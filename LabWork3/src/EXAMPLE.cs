namespace AuthExample
{
    #region Interfaces
    internal interface ITokenizer
    {
        public string CreateToken(string input);
        public void Verify(string input);
    }

    internal interface IPasswordEncoder
    {
        public string Encode(string input);
        public bool Verify(string fingerprint, string input);
    }

    internal interface IUserRepository
    {
        public UserDTO GetUserByLogin(string login);
        public UserDTO GetUserByEmail(string email);

        public void AddUser(UserDTO user);

        // ...
    }
    #endregion Interfaces


    internal class AuthService
    {
        private IUserRepository _userRepository;
        private IPasswordEncoder _encoder;
        private ITokenizer _tokenizer;

        public AuthService(IUserRepository userRepository, IPasswordEncoder encoder, ITokenizer tokenizer) 
        {
            _userRepository = userRepository;
            _encoder = encoder;
            _tokenizer = tokenizer;
        }


        public IToken Login(LoginRequest request)
        {
            var user = _userRepository.GetUserByLogin(request.Login);
            if (user is null)
                throw new Exception("Пользователя с указанным логином не существует");

            // ...

            IToken token = _tokenizer.CreateToken("SomeArgument");
            return token;
        }

        public UserDTO Register(RegistrationRequest request)
        {
            var user = _userRepository.GetUserByLogin(request.Login);
            if (user is not null)
                throw new Exception("Пользователь с указанным логином уже существует");

            user = _userRepository.GetUserByEmail(request.Email);
            if (user is not null)
                throw new Exception("Указанная электронная почта уже используется");

            // ...

            var fingerprint = _encoder.Encode(request.Password);
            var userDTO = new UserDTO()
            {
                Login = request.Login,
                Email = request.Email,
                Password = fingerprint
            };

            // ...

            _userRepository.AddUser(userDTO);
            return userDTO;
        }
    }



    internal class RegistrationRequest
    {
        public string Login { get; }
        public string Email { get; }
        public string Password { get; }
    }

    internal class LoginRequest
    {
        public string Login { get; }
        public string Password { get; }
    }

    internal class UserDTO
    {
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // ...
    }
}
