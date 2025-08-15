using AppointmentApp.API.Context;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Entities;
using AppointmentApp.API.Enums;
using AppointmentApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApp.API.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;

    public BookingService(AppDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(CreateBookingRequest req, Guid currentUserId, CancellationToken ct)
    {
        // 1) Yetki: isteği gönderen taraf coach ya da student olmalı
        var currentUser = await _db.Users.FindAsync([req.StudentId], ct) ?? 
                          await _db.Users.FindAsync([req.CoachId], ct);
        // (İstersen Me/claims üzerinden rollerini de kontrol edebilirsin)

        // 2) Servis + süre
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == req.ServiceId && s.CoachId == req.CoachId, ct)
            ?? throw new KeyNotFoundException("Service not found for coach");

        var endsAt = req.StartsAt.AddMinutes(service.DurationMin);

        // 3) Çakışma kontrolü (aynı coach için)
        bool overlap = await _db.Bookings.AnyAsync(b =>
            b.CoachId == req.CoachId &&
            b.Status == BookingStatus.Confirmed && // sadece onaylılarla çakışmayı engelle
            b.StartsAt < endsAt && req.StartsAt < b.EndsAt, ct);

        if (overlap) throw new InvalidOperationException("Time slot overlaps with another booking");

        // 4) (Opsiyonel) Availability'e uyuyor mu? — MVP’de atlanabilir
        // İstersen ilgili güne uyan availability rule var mı diye kontrol eklenir.

        // 5) Kaydı oluştur
        var booking = new Booking
        {
            CoachId = req.CoachId,
            StudentId = req.StudentId,
            StartsAt = req.StartsAt,
            EndsAt = endsAt,
            Status = BookingStatus.Pending
        };

        _db.Bookings.Add(booking);

        // 6) Bildirim (karşı tarafa)
        var recipient = (currentUserId == req.StudentId) ? req.CoachId : req.StudentId;
        _db.Notifications.Add(new Notification
        {
            RecipientUserId = recipient,
            Message = $"Yeni randevu talebi: {req.StartsAt:yyyy-MM-dd HH:mm}"
        });

        await _db.SaveChangesAsync(ct);
        return booking.Id;
    }

    public async Task AcceptAsync(Guid bookingId, Guid currentUserId, CancellationToken ct)
    {
        var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct)
                ?? throw new KeyNotFoundException("Booking not found");

        // Sadece karşı taraf onaylayabilir:
        // Coach oluşturduysa student onaylar; student oluşturduysa coach onaylar.
        bool isCoach = currentUserId == b.CoachId;
        bool isStudent = currentUserId == b.StudentId;
        if (!isCoach && !isStudent) throw new UnauthorizedAccessException();

        // Karşı taraf kontrolünü basit tutalım (MVP): Pending ise herkes onaylayabilir
        if (b.Status != BookingStatus.Pending) throw new InvalidOperationException("Booking not pending");

        // Son çakışma kontrolü (art arda talepler geldiyse)
        bool overlap = await _db.Bookings.AnyAsync(o =>
            o.Id != b.Id &&
            o.CoachId == b.CoachId &&
            o.Status == BookingStatus.Confirmed &&
            o.StartsAt < b.EndsAt && b.StartsAt < o.EndsAt, ct);
        if (overlap) throw new InvalidOperationException("Now overlaps with another confirmed booking");

        b.Status = BookingStatus.Confirmed;

        // WhatsApp entegrasyonu ileride: şimdilik Notification
        _db.Notifications.Add(new Notification
        {
            RecipientUserId = b.StudentId,
            Message = $"Randevu onaylandı: {b.StartsAt:yyyy-MM-dd HH:mm}"
        });
        _db.Notifications.Add(new Notification
        {
            RecipientUserId = b.CoachId,
            Message = $"Randevu onaylandı: {b.StartsAt:yyyy-MM-dd HH:mm}"
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(Guid bookingId, Guid currentUserId, CancellationToken ct)
    {
        var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct)
                ?? throw new KeyNotFoundException("Booking not found");

        if (b.Status != BookingStatus.Pending) throw new InvalidOperationException("Booking not pending");

        b.Status = BookingStatus.Rejected;

        var recipient = (currentUserId == b.StudentId) ? b.CoachId : b.StudentId;
        _db.Notifications.Add(new Notification
        {
            RecipientUserId = recipient,
            Message = $"Randevu reddedildi: {b.StartsAt:yyyy-MM-dd HH:mm}"
        });

        await _db.SaveChangesAsync(ct);
    }
}