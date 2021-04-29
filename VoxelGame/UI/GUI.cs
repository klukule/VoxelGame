using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System.Collections.Generic;
using VoxelGame.Assets;
using VoxelGame.Rendering;
using VoxelGame.Rendering.Vertex;

namespace VoxelGame.UI
{
    /// <summary>
    /// IMGUI style UI system
    /// </summary>
    public static class GUI
    {
        private static Material _materialStandard;      // Material for standard UI rendering
        private static Material _materialSliced;        // Material for nine-patch rendering
        private static Material _materialText;          // Material for font rendering

        private static string _lastId = "";             // Last drawn element ID
        private static string _lastClickedId = "";      // Last clicked element ID
        private static int _elementCount;               // Drawn element count
        private static int _textCarret;                 // Text carret position
        private static bool mouseButtonClick = false;   // Is mouse button clicked
        /// <summary>
        /// Default label style
        /// </summary>
        public static GUIStyle DefaultLabelStyle { get; }

        /// <summary>
        /// Default button type
        /// </summary>
        public static GUIStyle DefaultButtonStyle { get; }

        /// <summary>
        /// Default Text box style
        /// </summary>
        public static GUIStyle DefaultTextBoxStyle { get; }

        /// <summary>
        /// Current mouse position
        /// </summary>
        public static Vector2 MousePosition { get; private set; }


        static GUI()
        {
            // Load materials
            _materialStandard = AssetDatabase.GetAsset<Material>("Materials/GUI/GUI.mat");
            _materialSliced = AssetDatabase.GetAsset<Material>("Materials/GUI/GUI_Sliced.mat");
            _materialText = AssetDatabase.GetAsset<Material>("Materials/GUI/Text.mat");

            // Setup default styles
            DefaultLabelStyle = new GUIStyle()
            {
                FontSize = 32,
                Font = AssetDatabase.GetAsset<Font>("Fonts/Minecraft.ttf"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            DefaultButtonStyle = new GUIStyle()
            {
                FontSize = 32,
                Font = AssetDatabase.GetAsset<Font>("Fonts/Minecraft.ttf"),
                HorizontalAlignment = HorizontalAlignment.Middle,
                VerticalAlignment = VerticalAlignment.Middle,
                SlicedBorderSize = 2f,
                Normal = new GUIStyleOption()
                {
                    Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/btn_normal.png"),
                    TextColor = Color4.White
                },
                Hover = new GUIStyleOption()
                {
                    Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/btn_hover.png"),
                    TextColor = Color4.White
                },
                Active = new GUIStyleOption()
                {
                    Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/btn_hover.png"),
                    TextColor = Color4.White
                }
            };

            DefaultTextBoxStyle = new GUIStyle()
            {
                FontSize = 32,
                Font = AssetDatabase.GetAsset<Font>("Fonts/Minecraft.ttf"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Middle,
                AlignmentOffset = new Vector2(12, 0),
                SlicedBorderSize = 2f,
                Normal = new GUIStyleOption()
                {
                    Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/txt_normal.png"),
                    TextColor = Color4.White
                },
                Hover = new GUIStyleOption()
                {
                    Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/txt_hover.png"),
                    TextColor = Color4.White
                },
                Active = new GUIStyleOption()
                {
                    Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/txt_hover.png"),
                    TextColor = Color4.White
                }
            };

            Program.Window.MouseMove += (sender, args) => { MousePosition = new Vector2(args.Position.X, args.Position.Y); };

            Program.Window.MouseDown += (sender, args) =>
            {
                if (args.Button == MouseButton.Left)
                    mouseButtonClick = true;
                _lastClickedId = "";
            };

            Program.Window.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter)
                    _lastClickedId = "";
            };
        }


        /// <summary>
        /// Begin frame
        /// </summary>
        public static void BeginFrame()
        {
            _elementCount = 0;
        }

        /// <summary>
        /// End frame
        /// </summary>
        public static void EndFrame()
        {
            mouseButtonClick = false;
        }

        /// <summary>
        /// Stores active element ID
        /// </summary>
        /// <param name="id">ID</param>
        public static void PushID(string id)
        {
            if (string.IsNullOrEmpty(_lastId))
                _lastId = id;
        }

        /// <summary>
        /// Clears stored element ID
        /// </summary>
        public static void ClearID() => _lastId = "";

        #region Standard Button (Activates on click) - TODO: Icon version
        /// <summary>
        /// Draw new button using default style
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="size">Button area</param>
        /// <returns>True if button was clicked; otherwise false</returns>
        public static bool Button(string text, Rect size) => Button(text, size, DefaultButtonStyle);

        /// <summary>
        /// Draw new button
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="size">Button area</param>
        /// <param name="style">Button style</param>
        /// <returns>True if button was clicked; otherwise false</returns>
        public static bool Button(string text, Rect size, GUIStyle style)
        {
            string id = (_elementCount + 1).ToString();
            PushID(id);
            bool up = Mouse.GetState().IsButtonUp(MouseButton.Left);

            if (size.IsPointInside(MousePosition))
            {
                if (Mouse.GetState().IsButtonDown(MouseButton.Left)) // Active/clicked style
                {
                    DrawImage(style.Active.Background, size, false, true, style.SlicedBorderSize);
                    DrawText(text, size, style, false, true);
                    _lastClickedId = id; // Store ID as last clicked
                }
                else // Hover style
                {
                    DrawImage(style.Hover.Background, size, true, false, style.SlicedBorderSize);
                    DrawText(text, size, style, true, false);
                }
            }
            else // Default style
            {
                DrawImage(style.Normal.Background, size, false, false, style.SlicedBorderSize);
                DrawText(text, size, style, false, false);
            }

            bool wasClicked = id == _lastClickedId && up;

            if (wasClicked)
            {
                _lastClickedId = "";
                ClearID();
            }

            _elementCount++;
            return wasClicked;
        }
        #endregion

        #region Press Button (Activates on mouse down) - TODO: Text version
        /// <summary>
        /// Draw new button using default style
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="size">Button area</param>
        /// <returns>True if button was clicked; otherwise false</returns>
        public static bool PressButton(Texture image, Rect size) => PressButton(image, size, DefaultButtonStyle);

        /// <summary>
        /// Draw new button
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="size">Button area</param>
        /// <param name="style">Button style</param>
        /// <returns>True if button was clicked; otherwise false</returns>
        public static bool PressButton(Texture image, Rect size, GUIStyle style)
        {
            string id = (_elementCount + 1).ToString();
            PushID(id);

            bool active = false;
            if (size.IsPointInside(MousePosition))
            {
                if (mouseButtonClick) // Active style
                {
                    DrawImage(style.Active.Background, size, false, true, style.SlicedBorderSize);
                    DrawImage(image, size, false, true, style.SlicedBorderSize);
                    active = true;
                }
                else // Hover style
                {
                    DrawImage(style.Hover.Background, size, true, false, style.SlicedBorderSize);
                    DrawImage(image, size, false, true, style.SlicedBorderSize);
                }
            }
            else // Default style
            {
                DrawImage(style.Normal.Background, size, false, false, style.SlicedBorderSize);
                DrawImage(image, size, false, true, style.SlicedBorderSize);
            }

            _elementCount++;
            return active;
        }
        #endregion

        #region Label
        /// <summary>
        /// Draw text label using default style
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="size">Area</param>
        public static void Label(string text, Rect size) => Label(text, size, DefaultLabelStyle);

        /// <summary>
        /// Draw text label
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="size">Area</param>
        /// <param name="style">Style</param>
        public static void Label(string text, Rect size, GUIStyle style)
        {
            DrawText(text, size, style, false, false);
            _elementCount++;
        }
        #endregion

        #region Image
        /// <summary>
        /// Draw Image
        /// </summary>
        /// <param name="image">The image</param>
        /// <param name="size">Area</param>
        public static void Image(Texture image, Rect size)
        {
            DrawImage(image, size, false, false, 0);
            _elementCount++;
        }

        /// <summary>
        /// Draw image with custom UVs
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="size">Area</param>
        /// <param name="uvMin">Minimum UV</param>
        /// <param name="uvMax">Maximum UV</param>
        public static void Image(Texture image, Rect size, Vector2 uvMin, Vector2 uvMax)
        {
            Vector2[] uvs = new Vector2[4];
            uvs[0] = uvMin;
            uvs[1] = new Vector2(uvMin.X, uvMax.Y);
            uvs[2] = new Vector2(uvMax.X, uvMin.Y);
            uvs[3] = uvMax;

            DrawImage(image, size, false, false, 0, uvs);
            _elementCount++;
        }

        /// <summary>
        /// Draw image as nine-patch
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="size">Size</param>
        /// <param name="sliceSize">Border size</param>
        public static void Image(Texture image, Rect size, float sliceSize)
        {
            DrawImage(image, size, false, false, sliceSize);
            _elementCount++;
        }
        #endregion

        #region Textbox
        /// <summary>
        /// Draw textbox using default style
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="size">Area</param>
        public static void Textbox(ref string text, float maxLength, Rect size) => Textbox(ref text, maxLength, size, DefaultTextBoxStyle);

        /// <summary>
        /// Draw textbox with placeholder using default style
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="placeholder">Placeholder</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="size">Area</param>
        public static void Textbox(ref string text, string placeholder, float maxLength, Rect size) => Textbox(ref text, maxLength, size, DefaultTextBoxStyle, placeholder);

        /// <summary>
        /// Draw textbox
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="size">Area</param>
        /// <param name="style">Style</param>
        /// <param name="placeholder">Optional placeholder</param>
        public static void Textbox(ref string text, float maxLength, Rect size, GUIStyle style, string placeholder = "")
        {
            // TODO: Fix carret crash 
            string id = (_elementCount + 1).ToString();
            PushID(id);
            string theText = text;
            if (_lastClickedId == id)
            {
                var args = Input.LastKeyDown;
                if (args != null)
                {
                    var val = args.Key;
                    bool isUtility = false;

                    // Handle behavior keys
                    switch (val)
                    {
                        case Key.Unknown:
                            isUtility = true;
                            break;
                        case Key.BackSpace:
                            if (text.Length > _textCarret - 1 && _textCarret > 0)
                            {
                                text = text.Remove(_textCarret - 1, 1);
                                _textCarret--;
                            }

                            isUtility = true;
                            break;
                        case Key.Delete:
                            if (_textCarret < text.Length)
                                text = text.Remove(_textCarret, 1);

                            isUtility = true;
                            break;
                        case Key.Space:
                            text = text.Insert(_textCarret, " ");
                            _textCarret++;
                            isUtility = true;
                            break;
                        case Key.Left:
                            if (_textCarret > 0)
                                _textCarret--;

                            isUtility = true;
                            break;
                        case Key.Right:
                            if (_textCarret < text.Length)
                                _textCarret++;

                            isUtility = true;
                            break;

                        case Key.ShiftLeft:
                            isUtility = true;
                            break;
                        case Key.ShiftRight:
                            isUtility = true;
                            break;
                        case Key.CapsLock:
                            isUtility = true;
                            break;
                    }

                    if (!isUtility && theText.Length < maxLength) // If key isn't utility and we have space in text
                    {
                        // Process input
                        var chara = Input.LastKeyPress.ToString();
                        if (args.Modifiers == KeyModifiers.Shift)
                            chara = chara.ToUpper();
                        else
                            chara = chara.ToLower();

                        if (_textCarret > text.Length)
                            _textCarret = text.Length;

                        text = text.Insert(_textCarret, chara);
                        _textCarret++;
                    }
                }

                theText = text.Insert(_textCarret, "_");
            }

            Color4 startingColor = style.Normal.TextColor;

            // Replace with placeholder
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(placeholder) && _lastClickedId != id)
            {
                style.Normal.TextColor = Color4.Gray;
                style.Hover.TextColor = Color4.Gray;
                style.Active.TextColor = Color4.Gray;
                theText = placeholder;
            }

            if (size.IsPointInside(MousePosition)) // If hovering
            {
                if (Mouse.GetState().IsButtonDown(MouseButton.Left)) // If clicked
                {
                    DrawImage(style.Active.Background, size, false, true, style.SlicedBorderSize);
                    DrawText(theText, size, style, false, true);
                    _lastClickedId = id;
                }
                else
                {
                    DrawImage(style.Hover.Background, size, true, false, style.SlicedBorderSize);
                    DrawText(theText, size, style, true, false);
                }
            }
            else if (_lastClickedId == id) // If active
            {
                DrawImage(style.Active.Background, size, false, true, style.SlicedBorderSize);
                DrawText(theText, size, style, false, true);
            }
            else // Default
            {
                DrawImage(style.Normal.Background, size, false, false, style.SlicedBorderSize);
                DrawText(theText, size, style, false, false);
            }

            style.Normal.TextColor = startingColor;
            style.Hover.TextColor = startingColor;
            style.Active.TextColor = startingColor;
            _elementCount++;
        }
        #endregion

        /// <summary>
        /// Generates necessary vertices for displaying an image and renders it out
        /// </summary>
        /// <param name="image">Texture</param>
        /// <param name="size">Area</param>
        /// <param name="isHovered">Is hovered?</param>
        /// <param name="isActive">Is active?</param>
        /// <param name="sliceSize">Nine-patch border size</param>
        /// <param name="inUVs">UVs</param>
        private static void DrawImage(Texture image, Rect size, bool isHovered, bool isActive, float sliceSize, Vector2[] inUVs = null)
        {
            if (image == null) return;

            float x = ((size.X / Program.Window.Width) * 2) - 1;
            float y = (((Program.Window.Height - size.Y) / Program.Window.Height) * 2) - 1;
            float maxX = x + ((size.Width * 2) / Program.Window.Width);
            float maxY = y - ((size.Height * 2) / Program.Window.Height);

            List<uint> indices = new List<uint>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            vertices.Add(new Vector3(x, y, 0));
            vertices.Add(new Vector3(x, maxY, 0));
            vertices.Add(new Vector3(maxX, y, 0));
            vertices.Add(new Vector3(maxX, maxY, 0));

            if (inUVs != null && inUVs.Length == 4) // Custom UVs
            {
                uvs.AddRange(inUVs);
            }
            else // Default UVs
            {
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1));
            }

            indices.Add(0);
            indices.Add(1);
            indices.Add(2);

            indices.Add(1);
            indices.Add(3);
            indices.Add(2);

            GL.Disable(EnableCap.DepthTest);
            // Build new mesh
            Mesh mesh = new Mesh(new VertexContainer(vertices.ToArray(), uvs.ToArray()), indices.ToArray());

            if (sliceSize > 0) // Draw nine-patch
            {
                _materialSliced.SetTexture(0, image);
                _materialSliced.SetUniform("u_BorderSize", sliceSize);
                _materialSliced.SetUniform("u_Dimensions", new Vector2(size.Width, size.Height));
                Renderer.DrawNow(mesh, _materialSliced);
            }
            else // Draw regular
            {
                _materialStandard.SetTexture(0, image);
                Renderer.DrawNow(mesh, _materialStandard);
            }

            mesh.Dispose();
            GL.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Generates necessary vertices for displaying a text, performs basic wrapping and renders it out
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="size">Area</param>
        /// <param name="style">The style</param>
        /// <param name="isHovered">Is hovered</param>
        /// <param name="isActive">Is active</param>
        private static void DrawText(string text, Rect size, GUIStyle style, bool isHovered, bool isActive)
        {
            float winWidth = Program.Window.Width;
            float winHeight = Program.Window.Height;

            float x = ((size.X / winWidth) * 2) - 1;
            float y = (((winHeight - size.Y) / winHeight) * 2) - 1;

            uint indexCount = 0;
            List<uint> indices = new List<uint>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            List<string> lines = new List<string>();
            List<float> linesWidth = new List<float>();
            int activeLine = 0;
            lines.Add("");
            linesWidth.Add(0);

            // Scale
            float fontScale = style.FontSize / style.Font.FontSize;
            float scaleX = fontScale / winWidth;
            float scaleY = fontScale / winHeight;

            // Wrap the text
            for (int i = 0; i < text.Length; i++)
            {
                var character = style.Font.RequestGlyph(text[i]);

                if (text[i] == '\n') // New line
                {
                    lines.Add("");
                    linesWidth.Add(0);
                    activeLine++;
                }
                else
                {
                    // If character overflows current line, start new one
                    if (linesWidth[activeLine] + (character.AdvanceWidth * fontScale) > (size.Width * 2f))
                    {
                        lines.Add(text[i].ToString());
                        linesWidth.Add(character.AdvanceWidth);
                        activeLine++;
                    }
                    else // Can append to current line
                    {
                        lines[activeLine] += text[i];
                        linesWidth[activeLine] += character.AdvanceWidth;
                    }
                }
            }

            // Generate vertices for each line
            int index = 0;
            float lineOffset = 0;
            float charOffset = x;
            foreach (var line in lines)
            {
                for (int c = 0; c < line.Length; c++)
                {
                    var character = style.Font.RequestGlyph(line[c]);

                    float w = character.GlyphWidth * scaleX;
                    float h = character.GlyphHeight * scaleY;
                    float top = character.BearingTop * scaleY;

                    float xPos = charOffset + (GetXAlignment(index) / winWidth);
                    float yPos = y - lineOffset - (h - top) - (scaleY * style.Font.FontSize) - (GetYAlignment() / winHeight);
                    float xPosEnd = xPos + w;
                    float yPosEnd = yPos + h;

                    vertices.Add(new Vector3(xPos, yPos, 0));
                    vertices.Add(new Vector3(xPosEnd, yPos, 0));
                    vertices.Add(new Vector3(xPos, yPosEnd, 0));
                    vertices.Add(new Vector3(xPosEnd, yPosEnd, 0));

                    uvs.Add(new Vector2(character.U, character.V + character.GlyphHeight / style.Font.AtlasHeight));
                    uvs.Add(new Vector2(character.U + character.GlyphWidth / style.Font.AtlasWidth, character.V + character.GlyphHeight / style.Font.AtlasHeight));
                    uvs.Add(new Vector2(character.U, character.V));
                    uvs.Add(new Vector2(character.U + character.GlyphWidth / style.Font.AtlasWidth, character.V));

                    indices.Add(indexCount + 0);
                    indices.Add(indexCount + 1);
                    indices.Add(indexCount + 2);

                    indices.Add(indexCount + 3);
                    indices.Add(indexCount + 2);
                    indices.Add(indexCount + 1);

                    indexCount += 4;

                    charOffset += character.AdvanceWidth * scaleX;
                }

                charOffset = x;
                index++;
                lineOffset += style.FontSize * scaleY;
            }

            // TODO: Verify alignment calculations are working

            float GetXAlignment(int line)
            {
                float width = linesWidth[line];
                float rectWidth = size.Width;

                return style.HorizontalAlignment switch
                {
                    HorizontalAlignment.Left => style.AlignmentOffset.X,
                    HorizontalAlignment.Middle => style.AlignmentOffset.X + ((rectWidth * 2f) - (width * fontScale)) / 2f,
                    HorizontalAlignment.Right => style.AlignmentOffset.X + ((rectWidth * 2f) - (width * fontScale)),
                    _ => 0,
                };
            }

            float GetYAlignment()
            {
                float height = style.FontSize * lines.Count;
                float rectHeight = size.Height;

                return style.VerticalAlignment switch
                {
                    VerticalAlignment.Top => style.AlignmentOffset.Y,
                    VerticalAlignment.Middle => style.AlignmentOffset.Y + (rectHeight - height + style.Font.FontSize) / 2f,
                    VerticalAlignment.Bottom => style.AlignmentOffset.Y + (rectHeight - height) + (style.Font.FontSize / 2f) / 2f,
                    _ => 0,
                };
            }

            GL.Disable(EnableCap.DepthTest);
            // Generate mesh
            Mesh mesh = new Mesh(new VertexContainer(vertices.ToArray(), uvs.ToArray()), indices.ToArray());

            //Bind
            _materialText.SetTexture(0, style.Font.AtlasTexture);
            if (isActive)
                _materialText.SetUniform("u_Color", style.Active.TextColor.ToVector4());
            else if (isHovered)
                _materialText.SetUniform("u_Color", style.Hover.TextColor.ToVector4());
            else
                _materialText.SetUniform("u_Color", style.Normal.TextColor.ToVector4());

            // Draw
            Renderer.DrawNow(mesh, _materialText);

            mesh.Dispose();
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
