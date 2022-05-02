using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        public ICommandDataClient _dataClient { get; }
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(IPlatformRepo repository, IMapper mapper, ICommandDataClient dataClient, IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _dataClient = dataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var platformItems = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platformItem = _repository.GetPlatformById(id);
            if (platformItem != null)
            {
                return Ok(_mapper.Map<PlatformReadDto>(platformItem));
            }
            
            return NotFound();          
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform([FromBody] PlatformCreateDto platformDto)
        {
            try
            {
                var platform = _mapper.Map<Platform>(platformDto);
                _repository.CreatePlatform(platform);
                _repository.SaveChanges();

                var platformReadDto = _mapper.Map<PlatformReadDto>(platform);

                //sync message
                try
                {
                    await _dataClient.SendPlatformToCommant(platformReadDto);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"Could not send sync request to CommandService {e.Message}");
                }

                //async messge
                 try
                {
                    var platformPublishedhDto = _mapper.Map<PlatformPublishedhDto>(platform);
                    platformPublishedhDto.Event = "Platform_Published";
                    _messageBusClient.PublishNewPlatform(platformPublishedhDto);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"Could not send async request to CommandService {e.Message}");
                }

                return CreatedAtRoute(nameof(GetPlatformById), new {Id = platform.Id}, "created");
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}