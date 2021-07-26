// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class UploadScreenshotRequest : APIRequest<APIScreenshot>
    {
        private readonly byte[] screenshot;

        public event APIProgressHandler Progressed;

        public UploadScreenshotRequest(byte[] screenshot)
        {
            this.screenshot = screenshot;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            req.AddFile(@"screenshot", screenshot);
            req.DownloadProgress += requestProgress;
            return req;
        }

        protected override string Target => @"screenshots";

        private void requestProgress(long current, long total) => API.Schedule(() => Progressed?.Invoke(current, total));
    }
}
