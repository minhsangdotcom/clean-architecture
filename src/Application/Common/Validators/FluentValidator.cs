using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using FluentValidation;

namespace Application.Common.Validators;

public abstract class FluentValidator<T> : AbstractValidator<T>
{
    public FluentValidator(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        ApplyRules(contextAccessor, translator);
        ApplyRules(translator);
    }

    protected abstract void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    );

    protected abstract void ApplyRules(IMessageTranslatorService translator);
}
