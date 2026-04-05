using SkiaSharp;
using Svg.Skia;

namespace HomeMediaRemote.Status.Windows
{
	internal class SvgRenderer
	{
		private const float ShadowDx = 0f;
		private const float ShadowDy = 4f;
		private const float ShadowBlur = 4f;
		private static readonly SKColor ShadowColor = new(0, 0, 0, 128);

		public static Bitmap RenderToBitmap(string svgContent, Size size, bool addShadow)
		{
			using var skSvg = new SKSvg();
			skSvg.FromSvg(svgContent);
			if (skSvg.Picture == null)
			{
				throw new InvalidOperationException("Failed to parse SVG");
			}

			using var surface = SKSurface.Create(new SKImageInfo(size.Width, size.Height));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.Transparent);

			var bounds = skSvg.Picture.CullRect;

			if (addShadow)
			{
				var margin = ShadowBlur * 2 + Math.Max(Math.Abs(ShadowDx), Math.Abs(ShadowDy));
				var scaleX = (size.Width - margin * 2) / bounds.Width;
				var scaleY = (size.Height - margin * 2) / bounds.Height;
				var scale = Math.Min(scaleX, scaleY);

				canvas.Translate(margin, margin);
				canvas.Scale(scale);

				using var shadowImageFilter = SKImageFilter.CreateDropShadow(
					ShadowDx / scale, ShadowDy / scale,
					ShadowBlur / scale, ShadowBlur / scale,
					ShadowColor)
				;

				using var shadowPaint = new SKPaint
				{
					ImageFilter = shadowImageFilter
				};

				canvas.DrawPicture(skSvg.Picture, shadowPaint);
			}
			else
			{
				var scaleX = size.Width / bounds.Width;
				var scaleY = size.Height / bounds.Height;
				var scale = Math.Min(scaleX, scaleY);

				var offsetX = (size.Width - bounds.Width * scale) / 2f;
				var offsetY = (size.Height - bounds.Height * scale) / 2f;

				canvas.Translate(offsetX, offsetY);
				canvas.Scale(scale);

				canvas.DrawPicture(skSvg.Picture);
			}

			using var image = surface.Snapshot();
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			using var stream = data.AsStream();
			using var tempBitmap = new Bitmap(stream);

			return new Bitmap(tempBitmap); // глубокая копия, стрим можно освобождать
		}
	}
}
