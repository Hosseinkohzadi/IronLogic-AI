using IronLogic.Application.DTOs;

namespace IronLogic.Application.Interfaces;

public interface IMuscleMeasurementService
{
    Task<MuscleMeasurement> LogMeasurementAsync(MuscleMeasurementRequest request);
}