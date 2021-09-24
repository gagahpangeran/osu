// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public class WikiInfobox : CompositeDrawable
    {
        public FillFlowContainer ChildContainer { get; }

        public WikiInfobox()
        {
            AutoSizeAxes = Axes.Y;
            Masking = true;
            BorderThickness = 1;
            CornerRadius = 4;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true,
                },
                ChildContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            BorderColour = colourProvider.Background4;
        }

        protected override void Update()
        {
            base.Update();
            Width = Math.Min(300, Parent.DrawWidth / 2);
        }
    }
}
