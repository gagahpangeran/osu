﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIBeatmapSet : IBeatmapSetOnlineInfo, IBeatmapSetInfo
    {
        [JsonProperty(@"covers")]
        public BeatmapSetOnlineCovers Covers { get; set; }

        [JsonProperty(@"id")]
        public int OnlineID { get; set; }

        [JsonProperty(@"status")]
        public BeatmapOnlineStatus Status { get; set; }

        [JsonProperty(@"preview_url")]
        public string Preview { get; set; } = string.Empty;

        [JsonProperty(@"has_favourited")]
        public bool HasFavourited { get; set; }

        [JsonProperty(@"play_count")]
        public int PlayCount { get; set; }

        [JsonProperty(@"favourite_count")]
        public int FavouriteCount { get; set; }

        [JsonProperty(@"bpm")]
        public double BPM { get; set; }

        [JsonProperty(@"nsfw")]
        public bool HasExplicitContent { get; set; }

        [JsonProperty(@"spotlight")]
        public bool FeaturedInSpotlight { get; set; }

        [JsonProperty(@"video")]
        public bool HasVideo { get; set; }

        [JsonProperty(@"storyboard")]
        public bool HasStoryboard { get; set; }

        [JsonProperty(@"submitted_date")]
        public DateTimeOffset Submitted { get; set; }

        [JsonProperty(@"ranked_date")]
        public DateTimeOffset? Ranked { get; set; }

        [JsonProperty(@"last_updated")]
        public DateTimeOffset? LastUpdated { get; set; }

        [JsonProperty("ratings")]
        public int[] Ratings { get; set; } = Array.Empty<int>();

        [JsonProperty(@"track_id")]
        public int? TrackId { get; set; }

        [JsonProperty(@"hype")]
        public BeatmapSetHypeStatus? HypeStatus { get; set; }

        [JsonProperty(@"nominations_summary")]
        public BeatmapSetNominationStatus? NominationStatus { get; set; }

        public string Title { get; set; } = string.Empty;

        [JsonProperty("title_unicode")]
        public string TitleUnicode { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        [JsonProperty("artist_unicode")]
        public string ArtistUnicode { get; set; } = string.Empty;

        /// <summary>
        /// The creator of this beatmap set.
        /// </summary>
        /// <remarks>
        /// This property is set differently depending on the API endpoint. When retrieved via <see cref="SearchBeatmapSetsRequest"/>,
        /// detailed user info is not included and the creator's ID and username are filled from the <see cref="AuthorID"/> and
        /// <see cref="AuthorString"/> properties. For other API endpoints, this property is set by the <see cref="author"/> setter.
        /// </remarks>
        public APIUser Author = new APIUser();

        /// <summary>
        /// Helper property to deserialize the detailed user info to <see cref="Author"/>
        /// </summary>
        /// <remarks>
        /// This setter implements special handling for deleted users. When received a user with ID 1, it indicates
        /// the original user has been deleted. In such cases, the existing <see cref="Author"/> data
        /// (filled from <see cref="AuthorID"/> and <see cref="AuthorString"/>) is preserved. For valid user,
        /// the provided user info replaces the existing <see cref="Author"/>.
        /// </remarks>
        [JsonProperty(@"user")]
        private APIUser author
        {
            set => Author = value.Id != 1 ? value : Author;
        }

        /// <summary>
        /// The ID of the beatmap set's creator.
        /// </summary>
        /// <remarks>
        /// Helper property to deserialize the ID to <see cref="Author"/>.
        /// </remarks>
        [JsonProperty(@"user_id")]
        public int AuthorID
        {
            get => Author.Id;
            set => Author.Id = value;
        }

        /// <summary>
        /// The username of the beatmap set's creator.
        /// </summary>
        /// <remarks>
        /// Helper property to deserialize the username to <see cref="Author"/>.
        /// </remarks>
        [JsonProperty(@"creator")]
        public string AuthorString
        {
            get => Author.Username;
            set => Author.Username = value;
        }

        [JsonProperty(@"availability")]
        public BeatmapSetOnlineAvailability Availability { get; set; }

        [JsonProperty(@"genre")]
        public BeatmapSetOnlineGenre Genre { get; set; }

        [JsonProperty(@"language")]
        public BeatmapSetOnlineLanguage Language { get; set; }

        [JsonProperty(@"current_nominations")]
        public BeatmapSetOnlineNomination[]? CurrentNominations { get; set; }

        [JsonProperty(@"related_users")]
        public APIUser[]? RelatedUsers { get; set; }

        public string Source { get; set; } = string.Empty;

        [JsonProperty(@"tags")]
        public string Tags { get; set; } = string.Empty;

        [JsonProperty(@"beatmaps")]
        public APIBeatmap[] Beatmaps { get; set; } = Array.Empty<APIBeatmap>();

        [JsonProperty(@"converts")]
        public APIBeatmap[]? Converts { get; set; }

        [JsonProperty(@"related_tags")]
        public APITag[]? RelatedTags { get; set; }

        private BeatmapMetadata metadata => new BeatmapMetadata
        {
            Title = Title,
            TitleUnicode = TitleUnicode,
            Artist = Artist,
            ArtistUnicode = ArtistUnicode,
            Author = new RealmUser
            {
                OnlineID = Author.OnlineID,
                Username = Author.Username
            },
            Source = Source,
            Tags = Tags,
        };

        #region Implementation of IBeatmapSetInfo

        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => Beatmaps;

        IBeatmapMetadataInfo IBeatmapSetInfo.Metadata => metadata;

        DateTimeOffset IBeatmapSetInfo.DateAdded => throw new NotImplementedException();
        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => throw new NotImplementedException();
        double IBeatmapSetInfo.MaxStarDifficulty => throw new NotImplementedException();
        double IBeatmapSetInfo.MaxLength => throw new NotImplementedException();
        double IBeatmapSetInfo.MaxBPM => BPM;

        #endregion

        public bool Equals(IBeatmapSetInfo? other) => other is APIBeatmapSet b && this.MatchesOnlineID(b);

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => OnlineID.GetHashCode();
    }
}
