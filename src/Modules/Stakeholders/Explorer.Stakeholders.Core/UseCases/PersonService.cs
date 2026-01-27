using AutoMapper;
using Explorer.BuildingBlocks.Core.Exceptions;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITouristRepository _touristRepository; 
    private readonly IFirstTimeXpService _firstTimeXpService; 
    private readonly IMapper _mapper;

    public PersonService(
        IPersonRepository personRepository,
        IUserRepository userRepository,
        ITouristRepository touristRepository, 
        IFirstTimeXpService firstTimeXpService,
        IMapper mapper)
    {
        _personRepository = personRepository;
        _userRepository = userRepository;
        _touristRepository = touristRepository;
        _firstTimeXpService = firstTimeXpService;
        _mapper = mapper;
    }

    public PersonDto Get(long personId)
    {
        var person = _personRepository.Get(personId);
        if (person == null)
            throw new KeyNotFoundException($"Person with ID {personId} not found.");

        var dto = _mapper.Map<PersonDto>(person);

        var user = _userRepository.Get(person.UserId);
        dto.IsActive = user?.IsActive ?? false;

        return dto;
    }

    public PersonDto Update(PersonDto personDto)
    {
        var personId = personDto.UserId;
        var person = _personRepository.Get(personId);
        if (person == null)
            throw new KeyNotFoundException($"Person with ID {personId} not found.");

        // Proveri da li je ovo prvi put da postavlja profilnu sliku
        var wasProfilePictureEmpty = string.IsNullOrWhiteSpace(person.ProfilePictureUrl);
        var isNowSettingProfilePicture = !string.IsNullOrWhiteSpace(personDto.ProfilePictureUrl);

        person.Update(
            personDto.Name,
            personDto.Surname,
            personDto.Email,
            personDto.ProfilePictureUrl,
            personDto.Biography,
            personDto.Quote
        );

        var updatedPerson = _personRepository.Update(person);

        // Ako je prvi put postavio sliku, dodeli XP
        if (wasProfilePictureEmpty && isNowSettingProfilePicture)
        {
            var tourist = _touristRepository.Get(personId);
            if (tourist != null)
            {
                _firstTimeXpService.TryAwardFirstProfilePicture(tourist.Id, person.Id);
            }
        }

        return _mapper.Map<PersonDto>(updatedPerson);
    }

    public PersonDto Create(AccountRegistrationDto dto)
    {
        if (_personRepository.EmailExists(dto.Email))
            throw new EntityValidationException("Email already exists.");

        if (!dto.Email.Contains("@"))
            throw new EntityValidationException("Email must contain '@'.");

        if (!dto.Email.EndsWith("@gmail.com"))
            throw new EntityValidationException("Email must be a valid @gmail.com address.");

        var role = Enum.Parse<UserRole>(dto.Role, true);

        if (_userRepository.Exists(dto.Username))
            throw new EntityValidationException("Username already exists in users.");

        var user = new User(dto.Username, dto.Password, role, true);
        user = _userRepository.Create(user);

        var person = new Person(user.Id, dto.Name, dto.Surname, dto.Email);
        person = _personRepository.Create(person);

        return _mapper.Map<PersonDto>(person);
    }

    public List<PersonDto> GetAll(long currentPersonId)
    {
        var people = _personRepository.GetAll();
        var filtered = people.Where(p => p.UserId != currentPersonId);
        var dtos = new List<PersonDto>();

        foreach (var person in filtered)
        {
            var user = _userRepository.Get(person.UserId);
            var dto = _mapper.Map<PersonDto>(person);

            if (user != null)
            {
                dto.Username = user.Username;
                dto.Role = user.Role.ToString();
                dto.IsActive = user.IsActive;
            }
            else
            {
                dto.Username = "Unknown";
                dto.Role = "Unknown";
                dto.IsActive = false;
            }

            dtos.Add(dto);
        }

        return dtos;
    }

    public void Block(long id)
    {
        var user = _userRepository.Get(id);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Role == UserRole.Administrator)
            throw new EntityValidationException("Cannot block administrator accounts.");

        user.Block();
        _userRepository.Update(user);
    }

    public void Unblock(long id)
    {
        var user = _userRepository.Get(id);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.Unblock();
        _userRepository.Update(user);
    }

    public List<PersonDto> GetAllTourists()
    {
        var people = _personRepository.GetAll();
        var dtos = new List<PersonDto>();

        foreach (var person in people)
        {
            var user = _userRepository.Get(person.UserId);

            if (user != null && user.Role == UserRole.Tourist)
            {
                var dto = _mapper.Map<PersonDto>(person);
                dto.Username = user.Username;
                dto.Role = user.Role.ToString();
                dto.IsActive = user.IsActive;
                dtos.Add(dto);
            }
        }

        return dtos;
    }

    public PersonDto GetPersonByUserId(long userId)
    {
        var person = _personRepository.GetByUserId(userId);
        if (person == null)
            throw new KeyNotFoundException($"Person with userId {userId} not found.");

        var dto = _mapper.Map<PersonDto>(person);

        var user = _userRepository.Get(person.UserId);
        if (user != null)
        {
            dto.Username = user.Username;
            dto.Role = user.Role.ToString();
            dto.IsActive = user.IsActive;
        }

        return dto;
    }
}