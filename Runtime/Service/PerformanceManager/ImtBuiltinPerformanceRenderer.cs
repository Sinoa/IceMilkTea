// zlib/libpng License
//
// Copyright (c) 2020 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using UnityEngine;

namespace IceMilkTea.Service
{
    public class ImtBuiltinPerformanceRenderer : ImtPerformanceRenderer
    {
        private ImtBuiltinPerformanceSensor sensor;
        private readonly ImtSquareReference backImage;
        private readonly ImtSquareReference barBackImage;
        private readonly ImtSquareReference earlyBar;
        private readonly ImtSquareReference fixedBar;
        private readonly ImtSquareReference preUpdateBar;
        private readonly ImtSquareReference updateBar;
        private readonly ImtSquareReference lateBar;
        private readonly ImtSquareReference renderTexBar;
        private readonly ImtSquareReference renderBar;
        private readonly PerformanceLabel earlyLabel;
        private readonly PerformanceLabel fixedLabel;
        private readonly PerformanceLabel preUpdateLabel;
        private readonly PerformanceLabel updateLabel;
        private readonly PerformanceLabel lateLabel;
        private readonly PerformanceLabel renderTexLabel;
        private readonly PerformanceLabel renderLabel;
        private readonly PerformanceLabel memoryLabel;
        private readonly float frameTime;



        public ImtBuiltinPerformanceRenderer(float frameTime)
        {
            this.frameTime = frameTime;

            backImage = CreateSquareReference();
            barBackImage = CreateSquareReference();
            earlyBar = CreateSquareReference();
            fixedBar = CreateSquareReference();
            preUpdateBar = CreateSquareReference();
            updateBar = CreateSquareReference();
            lateBar = CreateSquareReference();
            renderTexBar = CreateSquareReference();
            renderBar = CreateSquareReference();
            earlyLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());
            fixedLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());
            preUpdateLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());
            updateLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());
            lateLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());
            renderTexLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());
            renderLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());
            memoryLabel = new PerformanceLabel(CreateTextReference(), CreateTextReference(), CreateNumberReference());


            backImage.Size = new Vector2(300.0f, 60.0f);
            backImage.Color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
            barBackImage.Size = new Vector2(290.0f, 5.0f);
            barBackImage.Color = Color.black;

            earlyBar.Size = new Vector2(290.0f, 5.0f);
            earlyBar.Color = Color.blue;
            earlyLabel.SetText("Early", "ms");
            earlyLabel.SetLabelColor(earlyBar.Color);

            fixedBar.Size = new Vector2(290.0f, 5.0f);
            fixedBar.Color = Color.yellow;
            fixedLabel.SetText("Fixed", "ms");
            fixedLabel.SetLabelColor(fixedBar.Color);

            preUpdateBar.Size = new Vector2(290.0f, 5.0f);
            preUpdateBar.Color = Color.green;
            preUpdateLabel.SetText("PreUpdate", "ms");
            preUpdateLabel.SetLabelColor(preUpdateBar.Color);

            updateBar.Size = new Vector2(290.0f, 5.0f);
            updateBar.Color = Color.cyan;
            updateLabel.SetText("Update", "ms");
            updateLabel.SetLabelColor(updateBar.Color);

            lateBar.Size = new Vector2(290.0f, 5.0f);
            lateBar.Color = Color.grey;
            lateLabel.SetText("Late", "ms");
            lateLabel.SetLabelColor(lateBar.Color);

            renderTexBar.Size = new Vector2(290.0f, 5.0f);
            renderTexBar.Color = Color.magenta;
            renderTexLabel.SetText("RenderTex", "ms");
            renderTexLabel.SetLabelColor(Color.magenta);

            renderBar.Size = new Vector2(290.0f, 5.0f);
            renderBar.Color = Color.red;
            renderLabel.SetText("Render", "ms");
            renderLabel.SetLabelColor(renderBar.Color);

            memoryLabel.SetText("Memory", "MiB");
        }


        public override void Render()
        {
            if (sensor == null)
            {
                sensor = GetService().GetSensor<ImtBuiltinPerformanceSensor>();
                if (sensor == null)
                {
                    return;
                }
            }


            var earlyTime = sensor.EarlyUpdateProcessTime / 1000.0;
            var fixedTime = sensor.FixedUpdateProcessTime / 1000.0;
            var preUpdateTime = sensor.PreUpdateProcessTime / 1000.0;
            var updateTime = sensor.UpdateProcessTime / 1000.0;
            var lateTime = sensor.LateUpdateProcessTime / 1000.0;
            var renderTexTime = sensor.RenderTextureProcessTime / 1000.0;
            var renderTime = sensor.RenderProcessTime / 1000.0;


            earlyLabel.SetNumber(earlyTime);
            fixedLabel.SetNumber(fixedTime);
            preUpdateLabel.SetNumber(preUpdateTime);
            updateLabel.SetNumber(updateTime);
            lateLabel.SetNumber(lateTime);
            renderTexLabel.SetNumber(renderTexTime);
            renderLabel.SetNumber(renderTime);
            memoryLabel.SetNumber(sensor.VMMemoryUsage / 1000.0 / 1000.0);


            var graphics = GetGraphics();
            backImage.Position = new Vector2(5.0f, graphics.VirtualResolution.y - backImage.Size.y - 5.0f);
            barBackImage.Position = new Vector2(10.0f, graphics.VirtualResolution.y - barBackImage.Size.y - 57.0f);

            earlyLabel.SetOffset(new Vector2(7.0f, graphics.VirtualResolution.y - 15.0f));
            fixedLabel.SetOffset(new Vector2(7.0f, graphics.VirtualResolution.y - 25.0f));
            preUpdateLabel.SetOffset(new Vector2(7.0f, graphics.VirtualResolution.y - 35.0f));
            updateLabel.SetOffset(new Vector2(7.0f, graphics.VirtualResolution.y - 45.0f));

            lateLabel.SetOffset(new Vector2(157.0f, graphics.VirtualResolution.y - 15.0f));
            renderTexLabel.SetOffset(new Vector2(157.0f, graphics.VirtualResolution.y - 25.0f));
            renderLabel.SetOffset(new Vector2(157.0f, graphics.VirtualResolution.y - 35.0f));
            memoryLabel.SetOffset(new Vector2(157.0f, graphics.VirtualResolution.y - 45.0f));

            var xMax = 290.0f;
            earlyBar.Size = new Vector2(xMax * ((float)earlyTime / frameTime), earlyBar.Size.y);
            fixedBar.Size = new Vector2(xMax * ((float)fixedTime / frameTime), fixedBar.Size.y);
            preUpdateBar.Size = new Vector2(xMax * ((float)preUpdateTime / frameTime), preUpdateBar.Size.y);
            updateBar.Size = new Vector2(xMax * ((float)updateTime / frameTime), updateBar.Size.y);
            lateBar.Size = new Vector2(xMax * ((float)lateTime / frameTime), lateBar.Size.y);
            renderTexBar.Size = new Vector2(xMax * ((float)renderTexTime / frameTime), renderTexBar.Size.y);
            renderBar.Size = new Vector2(xMax * ((float)renderTime / frameTime), renderBar.Size.y);

            var xOffset = 7.0f;
            earlyBar.Position = new Vector2(xOffset, barBackImage.Position.y);
            xOffset += earlyBar.Size.x;
            fixedBar.Position = new Vector2(xOffset, barBackImage.Position.y);
            xOffset += fixedBar.Size.x;
            preUpdateBar.Position = new Vector2(xOffset, barBackImage.Position.y);
            xOffset += preUpdateBar.Size.x;
            updateBar.Position = new Vector2(xOffset, barBackImage.Position.y);
            xOffset += updateBar.Size.x;
            lateBar.Position = new Vector2(xOffset, barBackImage.Position.y);
            xOffset += lateBar.Size.x;
            renderTexBar.Position = new Vector2(xOffset, barBackImage.Position.y);
            xOffset += renderTexBar.Size.x;
            renderBar.Position = new Vector2(xOffset, barBackImage.Position.y);
        }



        public class PerformanceLabel
        {
            private ImtTextReference label;
            private ImtNumberReference number;
            private ImtTextReference unitLabel;



            public PerformanceLabel(ImtTextReference label, ImtTextReference unitLabel, ImtNumberReference number)
            {
                this.label = label;
                this.number = number;
                this.unitLabel = unitLabel;


                this.number.Position = new Vector2(55.0f, 0.0f);
                this.unitLabel.Position = new Vector2(110.0f, 0.0f);
            }


            public void SetLabelColor(Color color)
            {
                label.Color = color;
            }


            public void SetOffset(Vector2 offset)
            {
                label.Position = offset;
                number.Position = new Vector2(55.0f, 0.0f) + offset;
                unitLabel.Position = new Vector2(110.0f, 0.0f) + offset;
            }


            public void SetText(string labelName, string unitText)
            {
                label.Text = labelName;
                unitLabel.Text = unitText;
            }


            public void SetNumber(double value)
            {
                number.Number = value;
            }
        }
    }
}