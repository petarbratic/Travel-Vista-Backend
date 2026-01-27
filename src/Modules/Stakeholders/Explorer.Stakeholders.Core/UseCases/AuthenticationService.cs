using Explorer.BuildingBlocks.Core.Exceptions;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Explorer.Stakeholders.Core.UseCases;

public class AuthenticationService : IAuthenticationService
{
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITouristRepository _touristRepository;
    private readonly IWelcomeBonusService _welcomeBonusService;
    private readonly string _googleClientId;

    public AuthenticationService(
        IUserRepository userRepository,
        IPersonRepository personRepository,
        ITokenGenerator tokenGenerator,
        IWalletRepository walletRepository,
        IWelcomeBonusService welcomeBonusService,
        ITouristRepository touristRepository,
        IConfiguration configuration)
    {
        _tokenGenerator = tokenGenerator;
        _userRepository = userRepository;
        _personRepository = personRepository;
        _walletRepository = walletRepository;
        _welcomeBonusService = welcomeBonusService;
        _touristRepository = touristRepository;
        _googleClientId = configuration["Google:ClientId"] ?? throw new ArgumentNullException("Google:ClientId not configured");
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

    public async Task<AuthenticationTokensDto> GoogleLoginAsync(GoogleLoginDto googleLogin)
    {
        // Validate Google token
        var payload = await GoogleJsonWebSignature.ValidateAsync(googleLogin.IdToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _googleClientId }
        });

        var email = payload.Email;
        var firstName = payload.GivenName ?? "Google";
        var lastName = payload.FamilyName ?? "User";

        // Check if user exists by email
        var existingPerson = _personRepository.GetByEmail(email);

        if (existingPerson != null)
        {
            // User exists - login
            var existingUser = _userRepository.Get(existingPerson.UserId);
            if (existingUser == null || !existingUser.IsActive)
            {
                throw new UnauthorizedAccessException("Account is not active");
            }
            var existingUserToken = _tokenGenerator.GenerateAccessToken(existingUser, existingPerson.Id);
            existingUserToken.IsNewUser = false;
            return existingUserToken;
        }

        // User doesn't exist - register as tourist
        var username = GenerateUniqueUsername(email);
        var randomPassword = Guid.NewGuid().ToString(); // Random password since they'll use Google to login

        var user = _userRepository.Create(new User(username, randomPassword, UserRole.Tourist, true));
        var person = _personRepository.Create(new Person(user.Id, firstName, lastName, email));
        var wallet = _walletRepository.Create(new Wallet(person.Id));

        _welcomeBonusService.CreateWelcomeBonus(person.Id);

        var tourist = _touristRepository.Create(new Tourist(person.Id));

        var newUserToken = _tokenGenerator.GenerateAccessToken(user, person.Id);
        newUserToken.IsNewUser = true;
        return newUserToken;
    }

    private string GenerateUniqueUsername(string email)
    {
        var baseUsername = email.Split('@')[0];
        var username = baseUsername;
        var counter = 1;

        while (_userRepository.Exists(username))
        {
            username = $"{baseUsername}{counter}";
            counter++;
        }

        return username;
    }
}
