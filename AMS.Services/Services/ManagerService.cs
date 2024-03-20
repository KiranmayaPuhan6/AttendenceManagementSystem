using AMS.DtoLibrary.DTO.ManagerDto;
using AMS.DtoLibrary.DTO.UserDto;
using AMS.Entities.Infrastructure.Repository.IRepository;
using AMS.Entities.Models.Domain.Entities;
using AMS.Services.Services.IServices;
using AMS.Services.Utility.ResponseModel;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AMS.Services.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IGenericRepository<Manager> _genericRepository;
        private readonly IResponseService _responseService;
        private readonly ILogger<ManagerService> _logger;
        private readonly IMapper _mapper;
        public ManagerService(IGenericRepository<Manager> genericRepository, IResponseService responseService,ILogger<ManagerService> logger, IMapper mapper)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseList<ManagerDto>> ReadAllManagerAsync()
        {
            var result = await _genericRepository.GetAllAsync();
            if(!result.Any())
            {
                return await _responseService.ResponseDtoFormatterAsync<ManagerDto>(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new List<ManagerDto>());
            }
            var managerDto = _mapper.Map<List<ManagerDto>>(result).ToList();
            return await _responseService.ResponseDtoFormatterAsync<ManagerDto>(true, (int)HttpStatusCode.OK, "Success", managerDto);
        }
    }
}
