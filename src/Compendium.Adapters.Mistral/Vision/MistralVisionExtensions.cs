// -----------------------------------------------------------------------
// <copyright file="MistralVisionExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Mistral.Vision;

/// <summary>
/// Ergonomic helpers for attaching image inputs to a <see cref="CompletionRequest"/>. Mistral's
/// Pixtral models accept multimodal content where each <c>content</c> field is an array of
/// <c>{ type: "text", text }</c> and <c>{ type: "image_url", image_url: { url } }</c> parts.
/// </summary>
/// <remarks>
/// The provider-agnostic <see cref="Message"/> shape only carries plain text. We piggy-back on
/// <see cref="CompletionRequest.AdditionalParameters"/> with a list of image URLs that the adapter
/// merges into the LAST user message at request-mapping time.
/// </remarks>
public static class MistralVisionExtensions
{
    /// <summary>Key inside <see cref="CompletionRequest.AdditionalParameters"/> carrying image URLs.</summary>
    public const string ImagesKey = "mistral.vision.images";

    /// <summary>
    /// Attaches one or more image URLs to the last user message. Use a HTTPS URL or a
    /// <c>data:image/...</c> base64-encoded data URL.
    /// </summary>
    /// <param name="request">The request to clone.</param>
    /// <param name="imageUrls">Image URLs (https:// or data:image/...;base64,...).</param>
    public static CompletionRequest WithImages(
        this CompletionRequest request,
        params string[] imageUrls)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(imageUrls);
        if (imageUrls.Length == 0)
        {
            return request;
        }
        foreach (var url in imageUrls)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrls));
            }
        }

        var dict = new Dictionary<string, object>(request.AdditionalParameters ?? new Dictionary<string, object>())
        {
            [ImagesKey] = imageUrls.ToList()
        };
        return request with { AdditionalParameters = dict };
    }
}
