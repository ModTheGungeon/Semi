using System;
using MonoMod;
using UnityEngine;

namespace Semi.Patches {
	[MonoModPatch("global::dfButton")]
	public class dfButton : global::dfButton {
		[MonoModIgnore]
		public extern void autoSizeToText();

		[MonoModIgnore]
		private extern float getTextScaleMultiplier();

		[MonoModIgnore]
		private extern Color32 getTextColorForState();

		[MonoModIgnore]
		private extern Vector3 getVertAlignOffset(dfFontRendererBase textRenderer);

		private dfFontRendererBase obtainTextRenderer() {
			Vector2 vector = base.Size - new Vector2((float)this.padding.horizontal, (float)this.padding.vertical);
			Vector2 maxSize = (!this.autoSize) ? MaximumSize : (Vector2.one * 2.14748365E+09f);
			float num = base.PixelsToUnits();
			Vector3 vector2 = (this.pivot.TransformToUpperLeft(base.Size) + new Vector3((float)this.padding.left, (float)(-(float)this.padding.top))) * num;
			float num2 = this.TextScale * this.getTextScaleMultiplier();
			Color32 defaultColor = base.ApplyOpacity(this.getTextColorForState());
			dfFontRendererBase dfFontRendererBase = this.Font.ObtainRenderer();
			dfFontRendererBase.WordWrap = this.WordWrap;
			dfFontRendererBase.MultiLine = this.WordWrap;
			dfFontRendererBase.MaxSize = maxSize;
			dfFontRendererBase.PixelRatio = num;
			dfFontRendererBase.TextScale = num2;
			dfFontRendererBase.CharacterSpacing = 0;
			int num3 = this.textPixelOffset;
			if (this.state == dfButton.ButtonState.Hover) {
				num3 = this.hoverTextPixelOffset;
			}
			if (this.state == dfButton.ButtonState.Pressed) {
				num3 = this.downTextPixelOffset;
			}
			if (this.state == dfButton.ButtonState.Disabled) {
				num3 = this.downTextPixelOffset;
			}
			dfFontRendererBase.VectorOffset = vector2.Quantize(num) + new Vector3(0f, (float)num3 * num, 0f);
			dfFontRendererBase.TabSize = 0;
			dfFontRendererBase.TextAlign = ((!this.autoSize) ? this.TextAlignment : TextAlignment.Left);
			dfFontRendererBase.ProcessMarkup = true;
			dfFontRendererBase.DefaultColor = defaultColor;
			dfFontRendererBase.OverrideMarkupColors = false;
			dfFontRendererBase.Opacity = base.CalculateOpacity();
			dfFontRendererBase.Shadow = this.Shadow;
			dfFontRendererBase.ShadowColor = this.ShadowColor;
			dfFontRendererBase.ShadowOffset = this.ShadowOffset;
			dfDynamicFont.DynamicFontRenderer dynamicFontRenderer = dfFontRendererBase as dfDynamicFont.DynamicFontRenderer;
			if (dynamicFontRenderer != null) {
				dynamicFontRenderer.SpriteAtlas = base.Atlas;
				dynamicFontRenderer.SpriteBuffer = this.renderData;
			}
			if (this.vertAlign != dfVerticalAlignment.Top) {
				dfFontRendererBase.VectorOffset = this.getVertAlignOffset(dfFontRendererBase);
			}
			return dfFontRendererBase;
		}
	}
}
