namespace HallyuVault.Core.Abstractions
{

    public abstract class Parser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        protected readonly IValidator<TInput> Validator;

        protected Parser(IValidator<TInput> validator)
        {
            Validator = validator;
        }

        public Result<TOutput> Parse(TInput input)
        {
            var validationResult = Validator.Validate(input);
            if (validationResult.IsFailure)
            {
                return Result.Failure<TOutput>(validationResult.Error);
            }

            return ParseInternal(input);
        }

        protected abstract Result<TOutput> ParseInternal(TInput input);
    }
}
