using Explorer.BuildingBlocks.Core.Exceptions;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases;

public class AuthenticationService : IAuthenticationService
{
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITouristRepository _touristRepository; 
    private readonly IWelcomeBonusService _welcomeBonusService;

    public AuthenticationService(
        IUserRepository userRepository,
        IPersonRepository personRepository,
        ITokenGenerator tokenGenerator,
        IWalletRepository walletRepository,
        IWelcomeBonusService welcomeBonusService,
        ITouristRepository touristRepository)
    {
        _tokenGenerator = tokenGenerator;
        _userRepository = userRepository;
        _personRepository = personRepository;
        _walletRepository = walletRepository;
        _welcomeBonusService = welcomeBonusService;
        _touristRepository = touristRepository; 
    }

    public AuthenticationTokensDto Login(CredentialsDto credentials)
    {
        var user = _userRepository.GetActiveByName(credentials.Username);
        if (user == null || credentials.Password != user.Password)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        long personId;
        try
        {
            personId = _userRepository.GetPersonId(user.Id);
        }
        catch (KeyNotFoundException)
        {
            personId = 0;
        }

        return _tokenGenerator.GenerateAccessToken(user, personId);
    }

    public AuthenticationTokensDto RegisterTourist(AccountRegistrationDto account)
    {
        if (_userRepository.Exists(account.Username))
            throw new EntityValidationException("Provided username already exists.");

        // 1. Kreiraj User
        var user = _userRepository.Create(new User(account.Username, account.Password, UserRole.Tourist, true));

        // 2. Kreiraj Person
        var person = _personRepository.Create(new Person(user.Id, account.Name, account.Surname, account.Email));

        // 3. Kreiraj Wallet
        var wallet = _walletRepository.Create(new Wallet(person.Id));
        
        _welcomeBonusService.CreateWelcomeBonus(person.Id);
        
        // 4. Kreiraj Tourist entitet
        var tourist = _touristRepository.Create(new Tourist(person.Id));

        return _tokenGenerator.GenerateAccessToken(user, person.Id);
    }
}
