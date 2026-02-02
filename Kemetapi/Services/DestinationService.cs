using Kemet_api.DTOs.Destination;
using Kemet_api.Models;
using Kemet_api.Repositories;

namespace Kemet_api.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly IDestinationRepository _destinationRepository;
        private readonly IDashboardService _dashboardService;

        public DestinationService(IDestinationRepository destinationRepository, IDashboardService dashboardService)
        {
            _destinationRepository = destinationRepository;
            _dashboardService = dashboardService;
        }

        public async Task<IEnumerable<DestinationDto>> GetAllDestinationsAsync()
        {
            var destinations = await _destinationRepository.GetAllWithVirtualTourAsync();
            return destinations.Select(d => new DestinationDto
            {
                Id = d.Id,
                Name = d.Name,
                City = d.City,
                Description = d.Description,
                ImageUrl = d.ImageUrl,
                EstimatedPrice = d.EstimatedPrice,
                FromWorkingHours = d.FromWorkingHours,
                EndWorkingHours = d.EndWorkingHours,
                VrId = d.VirtualTour?.Vr_id,
                VrUrlImage = d.VirtualTour?.Vr_urlImage
            });
        }

        public async Task<DestinationDto?> GetDestinationByIdAsync(Guid id)
        {
            var destination = await _destinationRepository.GetByIdWithVirtualTourAsync(id);
            if (destination == null) return null;

            // Track view
            await _dashboardService.TrackEventAsync("DestinationView", destination.Name);

            return new DestinationDto
            {
                Id = destination.Id,
                Name = destination.Name,
                City = destination.City,
                Description = destination.Description,
                ImageUrl = destination.ImageUrl,
                EstimatedPrice = destination.EstimatedPrice,
                FromWorkingHours = destination.FromWorkingHours,
                EndWorkingHours = destination.EndWorkingHours,
                VrId = destination.VirtualTour?.Vr_id,
                VrUrlImage = destination.VirtualTour?.Vr_urlImage
            };
        }

        public async Task<DestinationDto> CreateDestinationAsync(CreateDestinationDto dto)
        {
            var destination = new Destination
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                City = dto.City,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                EstimatedPrice = dto.EstimatedPrice,
                FromWorkingHours = dto.FromWorkingHours,
                EndWorkingHours = dto.EndWorkingHours
            };

            if (!string.IsNullOrEmpty(dto.VrUrlImage))
            {
                destination.VirtualTour = new VirtualTour
                {
                    Vr_id = Guid.NewGuid(),
                    Vr_urlImage = dto.VrUrlImage,
                    Destination_id = destination.Id
                };
            }

            await _destinationRepository.AddAsync(destination);
            await _destinationRepository.SaveChangesAsync();

            return new DestinationDto
            {
                Id = destination.Id,
                Name = destination.Name,
                City = destination.City,
                Description = destination.Description,
                ImageUrl = destination.ImageUrl,
                EstimatedPrice = destination.EstimatedPrice,
                FromWorkingHours = destination.FromWorkingHours,
                EndWorkingHours = destination.EndWorkingHours,
                VrId = destination.VirtualTour?.Vr_id,
                VrUrlImage = destination.VirtualTour?.Vr_urlImage
            };
        }

        public async Task<DestinationDto?> UpdateDestinationAsync(Guid id, UpdateDestinationDto dto)
        {
            var existingDestination = await _destinationRepository.GetByIdWithVirtualTourAsync(id);
            if (existingDestination == null) return null;

            existingDestination.Name = dto.Name;
            existingDestination.City = dto.City;
            existingDestination.Description = dto.Description;
            existingDestination.ImageUrl = dto.ImageUrl;
            existingDestination.EstimatedPrice = dto.EstimatedPrice;
            existingDestination.FromWorkingHours = dto.FromWorkingHours;
            existingDestination.EndWorkingHours = dto.EndWorkingHours;

            if (!string.IsNullOrEmpty(dto.VrUrlImage))
            {
                if (existingDestination.VirtualTour == null)
                {
                    existingDestination.VirtualTour = new VirtualTour
                    {
                        Vr_id = Guid.NewGuid(),
                        Vr_urlImage = dto.VrUrlImage,
                        Destination_id = existingDestination.Id
                    };
                }
                else
                {
                    existingDestination.VirtualTour.Vr_urlImage = dto.VrUrlImage;
                }
            }

            await _destinationRepository.UpdateAsync(existingDestination);
            await _destinationRepository.SaveChangesAsync();

            return new DestinationDto
            {
                Id = existingDestination.Id,
                Name = existingDestination.Name,
                City = existingDestination.City,
                Description = existingDestination.Description,
                ImageUrl = existingDestination.ImageUrl,
                EstimatedPrice = existingDestination.EstimatedPrice,
                FromWorkingHours = existingDestination.FromWorkingHours,
                EndWorkingHours = existingDestination.EndWorkingHours,
                VrId = existingDestination.VirtualTour?.Vr_id,
                VrUrlImage = existingDestination.VirtualTour?.Vr_urlImage
            };
        }

        public async Task<bool> DeleteDestinationAsync(Guid id)
        {
            var destination = await _destinationRepository.GetByIdAsync(id);
            if (destination == null) return false;

            await _destinationRepository.DeleteAsync(destination);
            await _destinationRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddToFavoritesAsync(Guid userId, Guid destinationId)
        {
            var exists = await _destinationRepository.GetFavoriteAsync(userId, destinationId);
            if (exists != null) return false; // Already favorited

            var favorite = new UserFavorite
            {
                UserId = userId,
                DestinationId = destinationId
            };

            await _destinationRepository.AddFavoriteAsync(favorite);
            await _destinationRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid destinationId)
        {
            var favorite = await _destinationRepository.GetFavoriteAsync(userId, destinationId);
            if (favorite == null) return false;

            await _destinationRepository.RemoveFavoriteAsync(favorite);
            await _destinationRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DestinationDto>> GetUserFavoritesAsync(Guid userId)
        {
            var destinations = await _destinationRepository.GetFavoritesByUserIdAsync(userId);
            return destinations.Select(d => new DestinationDto
            {
                Id = d.Id,
                Name = d.Name,
                City = d.City,
                Description = d.Description,
                ImageUrl = d.ImageUrl,
                EstimatedPrice = d.EstimatedPrice,
                FromWorkingHours = d.FromWorkingHours,
                EndWorkingHours = d.EndWorkingHours,
                VrId = d.VirtualTour?.Vr_id,
                VrUrlImage = d.VirtualTour?.Vr_urlImage
            });
        }
    }
}
