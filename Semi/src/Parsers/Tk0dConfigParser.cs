using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Semi {
    public class Tk0dConfigParser {
        internal enum Mode {
            Collection,
            Animation
        }

        /// <summary>
        /// Exception thrown when the parser receives unexpected input.
        /// </summary>
        public class Tk0dConfigParserException : Exception {
            public Tk0dConfigParserException(int line, int character, string message = null) : base($"[at {line}:{character}] {message}") { }
        }

        /// <summary>
        /// Object representing the parsed collection.
        /// </summary>
        public struct ParsedCollection {
            public struct AttachPointData {
                public string DefinitionID;
                public string AttachPoint;
                public string Alias;
                public float X;
                public float Y;
                public float Z;
                public float Angle;
            }

            public struct Definition {
                public string ID;
                public int X;
                public int Y;
                public int W;
                public int H;
                public bool FlipH;
                public bool FlipV;
                public string SpritesheetOverride;
            }

            public string ID;
            public string Name;
            public bool Patch;
            public int UnitW;
            public int UnitH;
            public int SizeW;
            public int SizeH;
            public string SpritesheetPath;
            public Dictionary<string, Definition> Definitions;
            public Dictionary<string, List<AttachPointData>> AttachPoints;

            // for export usage
            public Dictionary<string, string> AttachPointAliases;

            public void Write(StreamWriter writer) {

            }
        }

        /// <summary>
        /// Object representing the parsed animation.
        /// </summary>
        public struct ParsedAnimation {
            public struct Frame {
                public string Definition;
                public bool OffGround;
                public bool Invulnerable;
            }

            public struct Clip {
                public string Name;
                public tk2dSpriteAnimationClip.WrapMode WrapMode;
                public int FPS;
                public string Prefix;

                public List<Frame> Frames;
            }

            public string ID;
            public string Name;
            public bool Patch;
            public string Collection;
            public int DefaultFPS;
            public Dictionary<string, Clip> Clips;
        }

        internal string Input;
        internal int Position = -1;
        internal Mode ParserMode = Mode.Collection;

        internal int CurLine = 0;
        internal int CurChar = 0;

        internal int LastLine = 0;
        internal int LastChar = 0;

        // coll+anim
        internal bool IDParsed = false;
        internal bool NameParsed = false;
        internal bool Patch = false;
        internal string PatchNamespace;

        // coll
        internal bool SizeParsed = false;
        internal bool UnitParsed = false;
        internal bool SpritesheetParsed = false;

        // anim
        internal bool DefaultFPSParsed = false;
        internal bool CollectionParsed = false;

        internal ParsedCollection Collection;
        internal ParsedAnimation Animation;

        internal Dictionary<string, string> AttachPointAliasMap;

        ParsedAnimation.Clip? CurrentClip = null;

        internal void Advance(int n = 1) {
            LastChar = CurChar;
            LastLine = CurLine;
            Position += 1;
            CurChar += 1;
            if (Peek() == '\n') {
                CurChar = 0;
                CurLine += 1;

            }
        }

        internal char? Peek(int n = 1) {
            if ((Position + n) >= Input.Length) return null;
            return Input[Position + n];
        }

        internal char? Read() {
            if (Position >= Input.Length) return null;
            Advance();
            return Input[Position];
        }

        internal string ReadUntil(params char[] n) {
            var builder = new StringBuilder();
            var line_prev = CurLine;
            var char_prev = CurChar;
            char? c = Peek();
            while (c != null && !n.Contains(c)) {
                builder.Append(c);
                Advance();
                c = Peek();
            }

            LastLine = line_prev;
            LastChar = char_prev;

            return builder.ToString();
        }

        internal string ReadRange(char min, char max) {
            var builder = new StringBuilder();
            var line_prev = CurLine;
            var char_prev = CurChar;
            char? c = Peek();
            while (c != null && c >= min && c <= max) {
                builder.Append(c);
                Advance();
                c = Peek();
            }

            LastLine = line_prev;
            LastChar = char_prev;

            return builder.ToString();
        }

        internal string ReadUntilWhitespace() {
            return ReadUntil(' ', '\t', '\n');
        }

        internal bool IsWhiteSpace(char c) {
            return c == ' ' || c == '\t';
        }

        internal void EatWhitespace() {
            var line_prev = CurLine;
            var char_prev = CurChar;
            var c = Peek();
            while (c != null && (c == ' ' || c == '\t')) {
                Advance(1);
                c = Peek();
            }
            LastLine = line_prev;
            LastChar = char_prev;
        }

        internal bool VerifyPropName(string name) {
            if (ParserMode == Mode.Collection) {
                return name == "id" || name == "name" || name == "unit" || name == "size" || name == "spritesheet" || name == "attachpoint" || name == "patch" || name == "namespace";
            } else {
                return name == "id" || name == "name" || name == "collection" || name == "defaultfps" || name == "patch" || name == "namespace";
            }
        }

        internal bool IsSizeProperty(string name) {
            return name == "unit" || name == "size";
        }

        internal bool IsIntProperty(string name) {
            return name == "defaultfps";
        }

        internal bool IsBoolProperty(string name) {
            return name == "patch";
        }

        internal void Throw(string reason, int? line = null, int? character = null) {
            throw new Tk0dConfigParserException(line ?? LastLine, character ?? LastChar, reason);
        }

        internal Tk0dConfigParser(Mode mode, string input) {
            ParserMode = mode;
            if (mode == Mode.Animation) Animation = new ParsedAnimation();
            else Collection = new ParsedCollection();
            Input = input;
            CurLine = 1;
            CurChar = 0;
            LastLine = 1;
            LastChar = 0;
        }

        internal IntVector2 ReadPair(char delim) {
            var wstr = ReadRange('0', '9');
            if (wstr.Length == 0) Throw($"Expected number");
            var sep = Read();
            if (sep != delim) Throw($"Expected size delimeter");
            var hstr = ReadRange('0', '9');
            if (hstr.Length == 0) Throw($"Expected number");
            var junk = ReadUntilWhitespace();
            if (junk.Length > 0) Throw($"Expected number pair, got '{junk}'");

            return new IntVector2(int.Parse(wstr), int.Parse(hstr));
        }

        internal int ReadInt() {
            var num = ReadRange('0', '9');
            if (num.Length == 0) Throw($"Expected number");
            var junk = ReadUntilWhitespace();
            if (junk.Length > 0) Throw($"Expected number");

            return int.Parse(num);
        }

        internal IntVector2 ReadSize() => ReadPair('x');
        internal IntVector2 ReadPosition() => ReadPair(',');

        internal void ReadProperty() {
            Advance(); // skip $
            var prop_line = CurLine;
            var prop_char = CurChar;
            var propname = ReadUntilWhitespace();
            if (!VerifyPropName(propname)) Throw($"Invalid property: '{propname}'");
            EatWhitespace();

            if (propname == "id" && IDParsed) Throw("Duplicate id property", prop_line, prop_char);
            else if (propname == "name" && NameParsed) Throw("Duplicate name property", prop_line, prop_char);
            else if (propname == "size" && SizeParsed) Throw("Duplicate size property", prop_line, prop_char);
            else if (propname == "unit" && UnitParsed) Throw("Duplicate unit property", prop_line, prop_char);
            else if (propname == "spritesheet" && SpritesheetParsed) Throw("Duplicate spritesheet property", prop_line, prop_char);
            else if (propname == "collection" && CollectionParsed) Throw("Duplicate collection property", prop_line, prop_char);
            else if (propname == "defaultfps" && DefaultFPSParsed) Throw("Duplicate defaultfps property", prop_line, prop_char);
            else if (propname == "patch" && Patch) Throw("Duplicate patch property", prop_line, prop_char);

            if (propname == "attachpoint") {
                if (AttachPointAliasMap == null) AttachPointAliasMap = new Dictionary<string, string>();

                var alias = ReadUntilWhitespace();
                if (alias.Length == 0) Throw("Expected attach point alias");
                if (AttachPointAliasMap.ContainsKey(alias)) Throw($"Duplicate attach point alias: {alias}");

                EatWhitespace();
                var target = ReadUntil('\n');
                target = target.Trim();
                if (target.Length == 0) Throw("Expected attach point name");

                AttachPointAliasMap[alias] = target;
            } else if (IsIntProperty(propname)) {
                if (propname == "defaultfps") {
                    Animation.DefaultFPS = ReadInt();
                }
            } else if (IsSizeProperty(propname)) {
                var size = ReadSize();

                if (propname == "size") {
                    Collection.SizeW = size.x;
                    Collection.SizeH = size.y;
                    SizeParsed = true;
                } else if (propname == "unit") {
                    Collection.UnitW = size.x;
                    Collection.UnitH = size.y;
                    UnitParsed = true;
                }
            } else if (IsBoolProperty(propname)) {
                if (propname == "patch") {
                    Patch = true;
                    if (ParserMode == Mode.Collection) Collection.Patch = true;
                    else Animation.Patch = true;
                }
            } else {
                var value = ReadUntil('\n');
                if (value.Length == 0) Throw("Expected string value");
                if (propname == "id") {
                    IDParsed = true;
                    if (ParserMode == Mode.Collection) Collection.ID = value;
                    else Animation.ID = value;
                } else if (propname == "name") {
                    NameParsed = true;
                    if (ParserMode == Mode.Collection) Collection.Name = value;
                    else Animation.Name = value;
                } else if (propname == "spritesheet") {
                    SpritesheetParsed = true;
                    Collection.SpritesheetPath = value;
                } else if (propname == "collection") {
                    CollectionParsed = true;
                    Animation.Collection = value;
                } else if (propname == "namespace") {
                    if (!Patch) Throw("$namespace can only be used after a $patch property");
                    PatchNamespace = value;
                }
            }
        }

        internal ParsedCollection.Definition ReadDefinition(string default_namespace) {
            var def = new ParsedCollection.Definition();

            def.ID = ReadUntilWhitespace();
            if (def.ID.Length == 0) Throw("Expected definition ID");
            if (def.ID.Contains(":") && !def.ID.StartsWithInvariant($"{default_namespace}:") && !Patch) Throw("Cannot specify a different namespace outside of $patch mode");
            if (PatchNamespace != null && !def.ID.Contains(":")) def.ID = $"{PatchNamespace}:{def.ID}";
            else if (!def.ID.Contains(":")) def.ID = $"@:{def.ID}";
            if (Collection.Definitions.ContainsKey(def.ID)) {
                Throw("Duplicate definition");
            }
            EatWhitespace();
            var pos = ReadPosition();
            def.X = pos.x;
            def.Y = pos.y;
            EatWhitespace();
            var c = Peek();
            if (c >= '0' && c <= '9') {
                var size = ReadSize();
                def.W = size.x;
                def.H = size.y;
                EatWhitespace();
            }

            string flag = ReadUntilWhitespace();
            var was_attach = false;
            while (flag.Length > 0 && flag != "override") {
                if (flag == "fliph") {
                    was_attach = false; def.FlipH = true;
                } else if (flag == "flipv") {
                    was_attach = false; def.FlipV = true;
                } else if (flag == "at") {
                    was_attach = true;
                    EatWhitespace();
                    var attachpoint = ReadUntilWhitespace();
                    if (attachpoint.Length == 0) Throw("Expected attach point alias");
                    string real_attachpoint;
                    if (!AttachPointAliasMap.TryGetValue(attachpoint, out real_attachpoint)) {
                        Throw($"Attach point alias {attachpoint} doesn't exist");
                    }
                    EatWhitespace();

                    var x = ReadUntil(',');
                    if (x.Length == 0) Throw("Expected X attach point coordinate");
                    var x_float = float.Parse(x);
                    Advance();
                    var y = ReadUntilWhitespace();
                    if (y.Length == 0) Throw("Expected Y attach point coordinate");
                    var y_float = float.Parse(y);

                    var attach_data = new ParsedCollection.AttachPointData {
                        DefinitionID = def.ID,
                        AttachPoint = real_attachpoint,
                        Alias = attachpoint,
                        X = x_float,
                        Y = y_float,
                        Z = 0f, // haven't seen a need for this but can be implemented like angle
                        Angle = 0f // set separately
                    };

                    if (Collection.AttachPoints == null) Collection.AttachPoints = new Dictionary<string, List<ParsedCollection.AttachPointData>>();
                    List<ParsedCollection.AttachPointData> attach_data_list;
                    if (!Collection.AttachPoints.TryGetValue(def.ID, out attach_data_list)) {
                        attach_data_list = Collection.AttachPoints[def.ID] = new List<ParsedCollection.AttachPointData>();
                    }
                    attach_data_list.Add(attach_data);
                } else if (flag == "angle") {
                    if (!was_attach) Throw("This option can only be used after an attach point specification");
                    was_attach = false;
                    EatWhitespace();
                    var angle = ReadUntilWhitespace();
                    if (angle.Length == 0) Throw("Expected angle");
                    var angle_float = float.Parse(angle);

                    if (Collection.AttachPoints == null || Collection.AttachPoints.Count == 0 || !Collection.AttachPoints.ContainsKey(def.ID)) {
                        Throw("Tried to set angle of last attach point data, but there are no attach points on this collection or definition");
                    }

                    var attach_data_list = Collection.AttachPoints[def.ID];
                    if (attach_data_list.Count == 0) {
                        Throw("Tried to set angle of last attach point data, but there are no attach points on this definition (even though it does have a list for them)");
                    }
                    var last_id = attach_data_list.Count - 1;

                    var attach_data = attach_data_list[last_id];
                    attach_data.Angle = angle_float;
                    attach_data_list[last_id] = attach_data;
                }
                EatWhitespace();
                flag = ReadUntilWhitespace();
            }

            if (flag == "override") {
                EatWhitespace();
                var path = ReadUntil('\n');
                if (path.Length == 0) Throw("Expected spritesheet path");
                def.SpritesheetOverride = path;
            }

            return def;
        }

        internal ParsedAnimation.Clip ReadClipStart() {
            var clip = new ParsedAnimation.Clip();

            var clip_marker = ReadUntilWhitespace();
            if (clip_marker != "clip") Throw("Malformed clip entry");

            EatWhitespace();

            clip.Name = ReadUntilWhitespace();
            if (clip.Name.Length == 0) Throw("Expected clip name");

            if (Animation.Clips.ContainsKey(clip.Name)) {
                Throw("Duplicate clip");
            }

            EatWhitespace();

            var p = Peek();
            while (p != null && p != ':') {
                var option = ReadUntilWhitespace();
                EatWhitespace();
                if (option.Length > 0) {
                    var value = ReadUntil(' ', '\t', '\n', ':');
                    if (value.Length == 0) Throw($"Expected value for clip property '{option}'");

                    switch (option) {
                    case "wrapmode":
                        clip.WrapMode = (tk2dSpriteAnimationClip.WrapMode)Enum.Parse(typeof(tk2dSpriteAnimationClip.WrapMode), value, true);
                        break;
                    case "fps":
                        var int_value = int.Parse(value);
                        if (int_value == 0 && value != "0") Throw($"Expected int, got '{value}'");
                        clip.FPS = int_value;
                        break;
                    case "prefix":
                        clip.Prefix = value;
                        break;
                    }
                }
                p = Peek();
            }

            EatWhitespace();
            var r = ReadUntilWhitespace();
            if (r != ":") Throw("Malformed clip definition");

            CurrentClip = clip;

            return clip;
        }

        internal ParsedAnimation.Frame ReadFrame() {
            var frame = new ParsedAnimation.Frame();

            var p = Peek();
            var def_id_builder = new StringBuilder();
            var pre_line = CurLine;
            var pre_char = CurChar;
            while (p != null && p != '\n') {
                if ((
                    Peek(1) == 'i' && Peek(2) == 'n' && Peek(3) == 'v' && Peek(4) == 'u' &&
                    Peek(5) == 'l' && Peek(6) == 'n' && Peek(7) == 'e' && Peek(8) == 'r' &&
                    Peek(9) == 'a' && Peek(10) == 'b' && Peek(11) == 'l' && Peek(12) == 'e'
                ) || (
                    Peek(1) == 'o' && Peek(2) == 'f' && Peek(3) == 'f' && Peek(4) == 'g' &&
                    Peek(5) == 'r' && Peek(6) == 'o' && Peek(7) == 'u' && Peek(8) == 'n' &&
                    Peek(9) == 'd'
                )) {
                    break;
                }

                if (!IsWhiteSpace(p.Value)) def_id_builder.Append(p);

                Advance();
                p = Peek();
            }
            LastLine = pre_line;
            LastLine = pre_char;
            var def_id = def_id_builder.ToString();
            if (def_id.Length == 0) Throw("Expected sprite definition ID");
            frame.Definition = def_id;

            p = Peek();
            while (p != null && p != '\n') {
                var flag = ReadUntilWhitespace();
                EatWhitespace();
                switch (flag) {
                case "invulnerable": frame.Invulnerable = true; break;
                case "offground": frame.OffGround = true; break;
                default: Throw($"Unknown frame flag '{flag}'"); break;
                }
                p = Peek();
            }

            return frame;
        }

        internal void ReadLine(string default_namespace) {
            EatWhitespace();
            if (Peek() == '\n') { Advance(); return; }

            if (Peek() == '$') {
                ReadProperty();
            } else if (ParserMode == Mode.Collection) {
                var def = ReadDefinition(default_namespace);
                Collection.Definitions[def.ID] = def;
            } else {
                if (CurrentClip == null) {
                    var clip = ReadClipStart();
                    clip.Frames = new List<ParsedAnimation.Frame>();
                    Animation.Clips[clip.Name] = clip;
                } else {
                    if (Peek(1) == 'c' && Peek(2) == 'l' && Peek(3) == 'i' && Peek(4) == 'p' && (Peek(5) == '\t' || Peek(5) == ' ')) {
                        var clip = ReadClipStart();
                        clip.Frames = new List<ParsedAnimation.Frame>();
                        Animation.Clips[clip.Name] = clip;
                    } else {
                        var frame = ReadFrame();
                        Animation.Clips[CurrentClip.Value.Name].Frames.Add(frame);
                    }
                }
            }

            EatWhitespace();
            var junk = ReadUntil('\n');
            if (junk.Length > 0) Throw($"Unexpected junk after line: '{junk}'");
            Advance(); // eat newline
        }

        internal void Parse(string default_namespace) {
            if (ParserMode == Mode.Animation) Animation.Clips = new Dictionary<string, ParsedAnimation.Clip>();
            else Collection.Definitions = new Dictionary<string, ParsedCollection.Definition>();

            while (Peek() != null) {
                ReadLine(default_namespace);
            }
        }

        /// <summary>
        /// Parses a file in Semi Collection format and produces a <c>ParsedCollection</c>.
        /// </summary>
        /// <returns>The parsed collection representation.</returns>
        /// <param name="data">Contents of the Semi Collection file.</param>
        public static ParsedCollection ParseCollection(string data, string default_namespace) {
            data = ConvertWindowsNewlinesToUnix(data);
            var parser = new Tk0dConfigParser(Mode.Collection, data);
            parser.Parse(default_namespace);
            return parser.Collection;
        }

        /// <summary>
        /// Parses a file in Semi Animation format and produces a <c>ParsedAnimation</c>.
        /// </summary>
        /// <returns>The parsed animation representation.</returns>
        /// <param name="data">Contents of the Semi Animation file.</param>
        public static ParsedAnimation ParseAnimation(string data, string default_namespace) {
            data = ConvertWindowsNewlinesToUnix(data);
            var parser = new Tk0dConfigParser(Mode.Animation, data);
            parser.Parse(default_namespace);
            return parser.Animation;
        }

        /// <summary>
        /// Takes an input string and replaces any Windows line endings with Unix line endings so that the parser doesn't encounter any errors.
        /// </summary>
        /// <returns>The input string except with Linux line endings.</returns>
        /// <param name="input">Input string to change line endings of.</param>
        public static string ConvertWindowsNewlinesToUnix(string input) {
            return input.Replace("\r\n", "\n");
        }
    }
}