using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using FluentValidation;

namespace Application.Common.Validators;

public abstract class FluentValidator<T> : AbstractValidator<T>
{
    public FluentValidator(
        IRequestContextProvider contextProvider,
        IMessageTranslator translator
    )
    {
        ApplyRules(contextProvider, translator);
        ApplyRules(translator);
    }

    protected abstract void ApplyRules(
        IRequestContextProvider contextProvider,
        IMessageTranslator translator
    );

    protected abstract void ApplyRules(IMessageTranslator translator);
}
