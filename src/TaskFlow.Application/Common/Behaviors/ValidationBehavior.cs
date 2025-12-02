using FluentValidation;
using MediatR;

namespace TaskFlow.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that automatically validates requests using FluentValidation.
/// Runs BEFORE the handler executes.
/// If validation fails, throws ValidationException and handler never runs.
/// </summary>
/// <remarks>
/// How it works:
/// 1. MediatR sends a request (command or query)
/// 2. This behavior intercepts it BEFORE the handler
/// 3. Finds all validators for the request type
/// 4. Runs all validators
/// 5. If any fail, throws exception with all validation errors
/// 6. If all pass, calls next() which executes the handler
/// 
/// Benefits:
/// - Automatic validation for all requests
/// - No need to manually call validators in handlers
/// - Consistent validation across entire application
/// - Clean handler code (no validation clutter)
/// </remarks>
/// <typeparam name="TRequest">The type of request being validated</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Constructor - DI injects all validators for TRequest.
    /// If no validators exist for TRequest, _validators will be empty (not null).
    /// </summary>
    /// <param name="validators">All FluentValidation validators for this request type</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Executes validation logic before calling the next behavior/handler.
    /// This is called automatically by MediatR for every request.
    /// </summary>
    /// <param name="request">The request being processed (command or query)</param>
    /// <param name="next">Delegate to call the next behavior or handler</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The response from the handler</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Step 1: Check if there are any validators for this request type
        // If no validators, skip validation and go straight to handler
        if (!_validators.Any())
        {
            return await next();
        }

        // Step 2: Create validation context
        // This provides the request object to validators
        var context = new ValidationContext<TRequest>(request);

        // Step 3: Run all validators asynchronously and collect results
        // Each validator returns a ValidationResult with any errors
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Step 4: Collect all validation failures from all validators
        // SelectMany flattens the list of errors from multiple validators
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null) // Filter out null failures
            .ToList();

        // Step 5: If there are any failures, throw exception
        // This stops execution - handler will NOT run
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // Step 6: Validation passed - continue to next behavior or handler
        // next() calls the next item in the pipeline (another behavior or the handler)
        return await next();
    }
}
