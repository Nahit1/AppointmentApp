using AppointmentApp.API.Context;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Entities;
using AppointmentApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApp.API.Services;

public class CoachCatalogService(AppDbContext db) : ICoachCatalogService
{
    public async Task<Guid> CreateServiceAsync(Guid coachId, CreateServiceRequest req, CancellationToken ct)
    {
        var svc = new Service {
            CoachId = coachId, Name = req.Name, DurationMin = req.DurationMin,
            BasePriceCents = req.BasePriceCents, Currency = req.Currency
        };
        db.Services.Add(svc);
        await db.SaveChangesAsync(ct);
        return svc.Id;
    }

    public async Task<Guid> CreateAvailabilityRuleAsync(Guid coachId, CreateAvailabilityRuleRequest req, CancellationToken ct)
    {
        var rule = new AvailabilityRule {
            CoachId = coachId,
            Days = req.Days,
            StartTime = TimeOnly.Parse(req.StartTime),
            EndTime = TimeOnly.Parse(req.EndTime),
            BufferMin = req.BufferMin,
            Timezone = string.IsNullOrWhiteSpace(req.Timezone) ? "Europe/Istanbul" : req.Timezone,
            ValidFrom = string.IsNullOrWhiteSpace(req.ValidFrom) ? null : DateOnly.Parse(req.ValidFrom),
            ValidTo   = string.IsNullOrWhiteSpace(req.ValidTo)   ? null : DateOnly.Parse(req.ValidTo)
        };
        if (rule.EndTime <= rule.StartTime) throw new ArgumentException("EndTime must be after StartTime.");
        db.AvailabilityRules.Add(rule);
        await db.SaveChangesAsync(ct);
        return rule.Id;
    }

    public async Task<List<SlotResponse>> GetSlotsAsync(Guid coachId, Guid serviceId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        if (to <= from) return [];

        // Service & duration
        var svc = await db.Services.FirstAsync(s => s.Id == serviceId && s.CoachId == coachId, ct);
        var rules = await db.AvailabilityRules.Where(r => r.CoachId == coachId).ToListAsync(ct);

        var result = new List<SlotResponse>();
        // Basit üretim: from..to aralığında her gün için kuralları uygula
        for (var day = from; day < to; day = day.AddDays(1))
        {
            var localDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(day, rules.FirstOrDefault()?.Timezone ?? "Europe/Istanbul").Date;
            var dow = localDate.DayOfWeek;

            foreach (var r in rules)
            {
                if (!r.Days.Contains(dow)) continue;

                // if (r.ValidFrom.HasValue && localDate < r.ValidFrom.Value) continue;
                // if (r.ValidTo.HasValue && localDate > r.ValidTo.Value) continue;

                // Günün zaman aralığında slotlar
                var start = new DateTime(localDate.Year, localDate.Month, localDate.Day, r.StartTime.Hour, r.StartTime.Minute, 0, DateTimeKind.Unspecified);
                var end   = new DateTime(localDate.Year, localDate.Month, localDate.Day, r.EndTime.Hour,   r.EndTime.Minute,   0, DateTimeKind.Unspecified);

                // TZ → UTC (DateTimeOffset)
                var tz = TimeZoneInfo.FindSystemTimeZoneById(r.Timezone);
                var startTz = new DateTimeOffset(start, tz.GetUtcOffset(start));
                var endTz   = new DateTimeOffset(end,   tz.GetUtcOffset(end));

                // Slot üret: [startTz, endTz) içinde Duration + Buffer
                var cursor = startTz;
                while (cursor.AddMinutes(svc.DurationMin) <= endTz)
                {
                    var slotStart = cursor;
                    var slotEnd   = cursor.AddMinutes(svc.DurationMin);

                    // TODO: Booking çakışması olduğunda elenecek (Sprint-3)
                    result.Add(new SlotResponse(slotStart, slotEnd));

                    cursor = slotEnd.AddMinutes(r.BufferMin);
                }
            }
        }
        // Basit sıralama/filtre: sadece istenen aralıkta kalsın
        return result.Where(s => s.StartsAt >= from && s.EndsAt <= to).OrderBy(s => s.StartsAt).ToList();
    }
}