using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

/// <summary>
/// Repository interface for EasyCars credential operations
/// </summary>
public interface IEasyCarsCredentialRepository
{
    /// <summary>
    /// Creates a new credential
    /// </summary>
    Task<EasyCarsCredential> CreateAsync(EasyCarsCredential credential);

    /// <summary>
    /// Gets credential by dealership ID
    /// </summary>
    Task<EasyCarsCredential?> GetByDealershipIdAsync(int dealershipId);

    /// <summary>
    /// Gets credential by ID
    /// </summary>
    Task<EasyCarsCredential?> GetByIdAsync(int id);

    /// <summary>
    /// Updates an existing credential
    /// </summary>
    Task<EasyCarsCredential> UpdateAsync(EasyCarsCredential credential);

    /// <summary>
    /// Deletes a credential
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if credentials exist for a dealership
    /// </summary>
    Task<bool> ExistsForDealershipAsync(int dealershipId);
}
