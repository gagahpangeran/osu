// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal partial class CurrentlyOnlineDisplay : CompositeDrawable
    {
        private const float search_textbox_height = 40;
        private const float padding = 10;

        private readonly IBindableDictionary<int, UserPresence> onlineUserPresences = new BindableDictionary<int, UserPresence>();
        private readonly Dictionary<int, OnlineUserPanel> userPanels = new Dictionary<int, OnlineUserPanel>();

        private SearchContainer<OnlineUserPanel> userFlow = null!;
        private BasicSearchTextBox searchTextBox = null!;

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        [Resolved]
        private UserLookupCache users { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = padding * 2 + search_textbox_height,
                    Colour = colourProvider.Background4,
                },
                new Container<BasicSearchTextBox>
                {
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING, Vertical = padding },
                    Child = searchTextBox = new BasicSearchTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = search_textbox_height,
                        ReleaseFocusOnCommit = false,
                        HoldFocus = true,
                        PlaceholderText = HomeStrings.SearchPlaceholder,
                    },
                },
                userFlow = new SearchContainer<OnlineUserPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding
                    {
                        Top = padding * 3 + search_textbox_height,
                        Bottom = padding,
                        Right = padding,
                        Left = padding,
                    },
                },
            };

            searchTextBox.Current.ValueChanged += text => userFlow.SearchTerm = text.NewValue;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onlineUserPresences.BindTo(metadataClient.UserPresences);
            onlineUserPresences.BindCollectionChanged(onUserPresenceUpdated, true);
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            searchTextBox.TakeFocus();
        }

        private void onUserPresenceUpdated(object? sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (var kvp in e.NewItems)
                    {
                        int userId = kvp.Key;

                        users.GetUserAsync(userId).ContinueWith(task =>
                        {
                            if (task.GetResultSafely() is APIUser user)
                                Schedule(() => userFlow.Add(userPanels[userId] = createUserPanel(user)));
                        });
                    }

                    break;

                case NotifyDictionaryChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var kvp in e.OldItems)
                    {
                        int userId = kvp.Key;
                        if (userPanels.Remove(userId, out var userPanel))
                            userPanel.Expire();
                    }

                    break;
            }
        });

        private OnlineUserPanel createUserPanel(APIUser user) =>
            new OnlineUserPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
            });

        public partial class OnlineUserPanel : CompositeDrawable, IFilterable
        {
            public readonly APIUser User;

            private PurpleRoundedButton spectateButton = null!;

            public IEnumerable<LocalisableString> FilterTerms { get; }

            [Resolved]
            private IPerformFromScreenRunner? performer { get; set; }

            [Resolved]
            private MetadataClient? metadataClient { get; set; }

            public bool FilteringActive { set; get; }

            public bool MatchingFilter
            {
                set
                {
                    if (value)
                        Show();
                    else
                        Hide();
                }
            }

            public OnlineUserPanel(APIUser user)
            {
                User = user;

                FilterTerms = new LocalisableString[] { User.Username };

                AutoSizeAxes = Axes.Both;
            }

            protected override void Update()
            {
                base.Update();

                // TODO: we probably don't want to do this every frame.
                var activity = metadataClient?.GetPresence(User.Id)?.Activity;

                switch (activity)
                {
                    default:
                        spectateButton.Enabled.Value = false;
                        break;

                    case UserActivity.InSoloGame:
                    case UserActivity.InMultiplayerGame:
                    case UserActivity.InPlaylistGame:
                    case UserActivity.PlayingDailyChallenge:
                        spectateButton.Enabled.Value = true;
                        break;
                }
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(2),
                        Width = 290,
                        Children = new Drawable[]
                        {
                            new UserGridPanel(User)
                            {
                                RelativeSizeAxes = Axes.X,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre
                            },
                            spectateButton = new PurpleRoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Text = "Spectate",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Action = () => performer?.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(User))),
                            }
                        }
                    },
                };
            }
        }
    }
}
