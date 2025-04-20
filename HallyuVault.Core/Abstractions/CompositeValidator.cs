namespace HallyuVault.Core.Abstractions
{
    public class CompositeValidator<TInput> : IValidator<TInput>
    {
        private readonly IEnumerable<IValidator<TInput>> _validators;

        public CompositeValidator(IEnumerable<IValidator<TInput>> validators)
        {
            _validators = validators;
        }

        public Result Validate(TInput input)
        {
            foreach (var validator in _validators)
            {
                var result = validator.Validate(input);
                if (result.IsFailure)
                {
                    return result;
                }
            }

            return Result.Success();
        }
    }
}
