using IronLogic.Application.DTOs;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

public interface IMuscleMeasurementService
{
    Task<MuscleMeasurement> LogMeasurementAsync(MuscleMeasurementRequest request);
}