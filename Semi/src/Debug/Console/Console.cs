using System;
using System.Collections.Generic;
using System.Text;
using ModTheGungeon;
using SGUI;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi.DebugConsole {
    public partial class Console {
        public static Logger Logger = new Logger("Console");

		public SGroup Window;

		internal SGroup OutputBox {
			get {
				return (SGroup)Window[(int)WindowChild.OutputBox];
			}
		}
		internal SGroup AutocompleteBox {
			get {
				return (SGroup)Window[(int)WindowChild.AutocompleteBox];
			}
		}
		internal STextField InputBox {
			get {
				return (STextField)Window[(int)WindowChild.InputBox];
			}
		}

		private enum WindowChild {
			OutputBox,
			AutocompleteBox,
			InputBox
		}

		public Console() {
			AddDefaultCommands();

			Window = new SGroup {
				Background = new Color(0, 0f, 0f, 0.8f),
				Visible = false,

				OnUpdateStyle = elem => {
					elem.Fill(0);
				},

				Children = {
					new SGroup { // OutputBox
                        Background = new Color(0, 0, 0, 0),
						AutoLayout = (self) => self.AutoLayoutVertical,
						ScrollDirection = SGroup.EDirection.Vertical,
						OnUpdateStyle = (elem) => {
							elem.Fill(0);
							elem.Size -= new Vector2(0, elem.Backend.LineHeight);
						},
						Children = {
							new SLabel($"SemiLoader v{Semi.SemiLoader.VERSION}") {
								Foreground = new Color(0/255f, 161/255f, 231/255f)
							}
						},
					},
					new SGroup { // AutocompleteBox
                        Background = new Color(0.2f, 0.2f, 0.2f, 0.9f),
						AutoLayout = (self) => self.AutoLayoutVertical,
						ScrollDirection = SGroup.EDirection.Vertical,
						OnUpdateStyle = (elem) => {
							elem.Size.x = elem.Parent.InnerSize.x;
							elem.Size.y = elem.Parent.InnerSize.y / 10; // 10%
                            elem.Position.y = elem.Parent.InnerSize.y - elem.Parent[(int)WindowChild.InputBox].Size.y - elem.Size.y;
						},
						Children = {},
					},
					new STextField { // InputBox
                        OverrideTab = true,

						OnUpdateStyle = (elem) => {
							elem.Size.x = elem.Parent.InnerSize.x;
							elem.Position.x = 0;
							elem.Position.y = elem.Parent.InnerSize.y - elem.Size.y;
						},

						OnKey = (self, is_down, key) => {
							if (!is_down || key == KeyCode.Return || key == KeyCode.KeypadEnter) return;

							switch(key) {
							case KeyCode.Home:
								self.MoveCursor(0);
								break;
							case KeyCode.Escape:
							case KeyCode.F2:
								Hide();
								break;
							case KeyCode.Tab:
								break;
								// TODO @console probably not necessary but might be nice?
								DoAutoComplete();
								break;
							case KeyCode.UpArrow:
								History.MoveUp();
								self.MoveCursor(History.Entry.Length);
								break;
							case KeyCode.DownArrow:
								History.MoveDown();
								self.MoveCursor(History.Entry.Length);
								break;
							default:
								History.LastEntry = self.Text;
								History.CurrentIndex = History.LastIndex;
								break;
							}

							self.Text = History.Entry;
						},

						OnSubmit = (elem, text) => {
							if (text.Trim().Length == 0) return;
							History.Push();
							ExecuteCommandAndPrintResult(text);
						},
					}
				}
			};

			_Executor = new Parser.Executor((name, args, history_index) => {
				Command cmd = ResolveCommand(name);
				if (history_index == null && History.LastIndex > 0) {
					history_index = History.LastIndex - 1;
				}
				return cmd.Run(args, history_index);
			});

			NormalHistory = new CommandHistory(_Executor, _Parser);
		}

		public void Show() {
			Window.Visible = true;
			Window[(int)WindowChild.InputBox].Focus();
		}

		public void Hide() {
			Window.Visible = false;
			DiscardAutocomplete(dont_reset_text: true);
			History.LastEntry = Text = "";
		}

        public const char COMMAND_PATH_SEPARATOR = '/';
        public Dictionary<string, Command> Commands = new Dictionary<string, Command>();

        public string CurrentCommandText {
            set {
                if (!History.IsLastIndex) {
                    History.LastEntry = History.Entry;
                    History.CurrentIndex = History.LastIndex;
                }

                History.Entry = value;
                InputBox.Text = value;
            }

            get {
                return InputBox.Text;
            }
        }

        public CommandHistory History {
            get {
                return NormalHistory;
            }
            set {
                NormalHistory = value;
            }
        }

        public CommandHistory NormalHistory;

        public bool PrintUsedCommand = true;

        public class CommandResolutionException : Exception { public CommandResolutionException(string msg) : base(msg) { } }

        private Parser.Lexer _Lexer = new Parser.Lexer(); // a lexer for autocomplete purposes
        private Parser.Parser _Parser = new Parser.Parser();
        private Parser.Executor _Executor;

        public Command ResolveCommand(string path) {
            Command cmd;
            string cmd_str;

            if (TryResolveCommand(path, out cmd, out cmd_str)) return cmd;
            if (cmd_str == path) {
                throw new CommandResolutionException($"Command '{path}' doesn't exist.");
            } else {
                throw new CommandResolutionException($"Command '{path}' doesn't exist (because '{cmd_str}' doesn't exist).");
            }
        }

        public bool TryResolveCommand(string path, out Command cmd, out string result_command) {
            Command foundcmd = null;
            var dict = Commands;

            var split = path.Split(COMMAND_PATH_SEPARATOR);

            for (int i = 0; i < split.Length; i++) {
                var el = split[i];

                if (!dict.TryGetValue(el, out foundcmd)) {
                    var builder = new StringBuilder();
                    for (int j = 0; j <= i; j++) {
                        builder.Append(split[j]);
                        if (j != i) builder.Append(COMMAND_PATH_SEPARATOR);
                    }

                    cmd = foundcmd;
                    result_command = builder.ToString();
                    return false;
                }

                dict = foundcmd.SubCommands;
            }

            cmd = foundcmd;
            result_command = path;
            return true;
        }



        private bool _AutocompleteVisible {
            get { return AutocompleteBox.Visible; }
            set { AutocompleteBox.Visible = value; }
        }
        private int _AutocompleteIndex = 0;
        private string _AutocompletePreContent = null;



        public string Text {
            get { return InputBox.Text; }
            set { InputBox.Text = value; }
        }

        internal void DiscardAutocomplete(bool dont_reset_text = false) {
            if (!dont_reset_text) {
                if (_AutocompletePreContent == null) throw new InvalidOperationException("Tried to discard autocomplete with no content");

                Text = _AutocompletePreContent;
            }
            _AutocompleteVisible = false;
            _AutocompleteIndex = 0;
        }

        private bool _CharFitsInLiteralOrSeparatorToken(Parser.Token token, int pos) {
            var fits = false;
            switch (token.Type) {
            case Parser.TokenType.Literal:
                fits = token.FirstLine == 1 && token.LastLine == 1 && (token.FirstCharacter - 1) <= pos && (token.LastCharacter + 1) >= pos;
                break;
            case Parser.TokenType.Separator:
                fits = token.FirstLine == 1 && token.LastLine == 1 && token.FirstCharacter <= pos && token.LastCharacter >= pos;
                break;
            }

            return fits;
        }

        private Parser.Token? _FindCommandEnd(int command_start_idx, List<Parser.Token> tokens) {
            var command_start = tokens[command_start_idx];
            if (command_start.Type != Parser.TokenType.CommandStart) throw new ArgumentException("command_start_idx must point to a CommandStart token");

            Parser.Token? command_end = null;

            int depth = 1;
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                if (token.Type == Parser.TokenType.CommandStart) depth += 1;
                else if (token.Type == Parser.TokenType.CommandEnd) {
                    depth -= 1;
                    if (depth <= 0) {
                        command_end = token;
                        break;
                    }
                }
            }

            return command_end;
        }

        private bool _CharFitsInASTNode(int char_pos, Parser.ASTNode node, bool tight = false) {
            return node.Position.FirstLine == 1 &&
                       node.Position.LastLine == 1 &&
                       char_pos >= (node.Position.FirstChar - (
                           (node is Parser.Argument) ? 1 : 0
                       )) &&
                       char_pos <= (node.Position.LastChar - (
                           tight ? 1 : 0
                       ));
        }

        private bool _CharFitsBetweenASTNodes(int char_pos, Parser.ASTNode left, Parser.ASTNode right) {
            return left.Position.FirstLine == 1 &&
                       left.Position.LastLine == 1 &&
                       right.Position.FirstLine == 1 &&
                       right.Position.LastLine == 1 &&
                       char_pos >= left.Position.LastChar &&
                       char_pos <= right.Position.FirstChar;
        }

        private struct AutocompleteInfo {
            public int ArgIndex; // 0-indexed, 0 is command, 1+ is args
            public Parser.Command CommandNode;
            public Parser.ASTNode ASTNode;
            public int LastCharIndex;
            public int FirstCharIndex;

            public AutocompleteInfo(int arg_index, Parser.Command command_node, Parser.ASTNode ast_node) {
                ArgIndex = arg_index;
                CommandNode = command_node;
                ASTNode = ast_node;
                if (ASTNode != null) {
                    LastCharIndex = ASTNode.Position.LastChar;
                    FirstCharIndex = ASTNode.Position.FirstChar;
                } else {
                    LastCharIndex = CommandNode.Position.LastChar;
                    FirstCharIndex = CommandNode.Position.FirstChar;
                }
            }

            public override string ToString() {
                return $"[AutocompleteInfo] ArgIndex={ArgIndex} CommandNode={CommandNode} ASTNode={ASTNode} LastCharIndex={LastCharIndex}";
            }
        }

        private AutocompleteInfo _GetAutocompleteInfo(Parser.Command cmd) {
            var inbox = InputBox;

            var cursor_pos = inbox.CursorIndex; // CursorIndex is 0 based

            // TODO
            // tabbing at: e|[[echo e]cho cho] a b
            // weirdly jumps

            for (int i = 0; i < cmd.Name.Content.Count; i++) {
                var node = cmd.Name.Content[i];

                if (node is Parser.Command && _CharFitsInASTNode(cursor_pos, node, tight: true)) {
                    // forget that shit, just autocomplete this command thank you very much
                    return _GetAutocompleteInfo((Parser.Command)node);
                }
            }

            if (_CharFitsInASTNode(cursor_pos, cmd.Name)) {
                return new AutocompleteInfo(0, cmd, cmd.Name);
            }

            if (cmd.Args.Count >= 1) {
                if (_CharFitsBetweenASTNodes(cursor_pos, cmd.Name, cmd.Args[0])) {
                    return new AutocompleteInfo(1, cmd, cmd.Args[0]);
                } else {
                    int arg_index = 1;
                    for (int i = 0; i < cmd.Args.Count; i++) {
                        for (int j = 0; j < cmd.Args[i].Content.Count; j++) {
                            var node = cmd.Args[i].Content[j];

                            if (node is Parser.Command && _CharFitsInASTNode(cursor_pos, node, tight: true)) {
                                return _GetAutocompleteInfo((Parser.Command)node);
                            }
                        }

                        var arg = cmd.Args[i];
                        if (_CharFitsInASTNode(cursor_pos, arg)) {
                            return new AutocompleteInfo(arg_index, cmd, arg);
                        }

                        if ((i + 1) < cmd.Args.Count && _CharFitsBetweenASTNodes(cursor_pos, arg, cmd.Args[i + 1])) {
                            return new AutocompleteInfo(arg_index, cmd, cmd.Args[i + 1]);
                        }

                        arg_index += 1;
                    }
                }
            }

            return new AutocompleteInfo(cmd.Args.Count, cmd, null);
        }

        internal void DoAutoComplete() {
            var input_box = InputBox;
            var info = _GetAutocompleteInfo(_Parser.Parse(CurrentCommandText, lenient: true));
            input_box.MoveCursor(info.LastCharIndex, info.FirstCharIndex - 1); // CursorIndex is 0-indexed
        }

        public void Paragraph() {
            OutputBox.Children.Add(new SLabel());
        }

        public void PrintLine(string txt, bool log = false, Color? color = null) {
            var outputbox = OutputBox;
            //if (outputbox.Children.Count == 0) {
            var label = new SLabel(txt);
            if (color.HasValue) label.Foreground = color.Value;
            outputbox.Children.Add(label);
            //  return;
            //} else {
            //    var elem = outputbox.Children[outputbox.Children.Count - 1];

            //    if (elem is SLabel) ((SLabel)elem).Text += txt;
            //    else outputbox.Children.Add(new SLabel(txt));
            //}
            outputbox.ScrollPosition = new Vector2(0, outputbox.ContentSize.y);
            outputbox.UpdateStyle(); // SGUI BUG! without this scroll breaks with labels with a lot of newlines
            if (log) Logger.InfoPretty(txt);
        }

        public void PrintError(string txt, bool log = false) {
            if (txt.IndexOf('\n') == -1) {
                PrintLine(txt, color: new Color(255 / 255f, 110 / 255f, 110 / 255f));
            } else {
                foreach (var line in txt.Split('\n')) {
                    PrintError(line);
                }
            }
            if (log) Logger.ErrorPretty(txt);
        }

        public string ExecuteCommand(string cmd) {
            return _Executor.ExecuteCommand(_Parser.Parse(cmd));
        }

        public void ExecuteCommandAndPrintResult(string cmd) {
            if (PrintUsedCommand) PrintLine("> " + cmd, color: new Color(87 / 255f, 87 / 255f, 87 / 255f));
            try {
                PrintLine(ExecuteCommand(cmd));
            } catch (Exception e) {
                PrintError("Exception while executing command:");
                PrintError(e.Message);
                PrintError("More detailed info in the log.");

                Logger.Error($"Exception while running command '{cmd}':");
                Logger.Error(e.Message);
                var stlines = e.StackTrace.Split('\n');

                for (int i = 0; i < stlines.Length; i++) {
                    Logger.ErrorIndent(stlines[i]);
                }
            }
        }

        public Group AddGroup(Group group) {
            Commands[group.Name] = group;
            return group;
        }

        public Command AddGroup(string name) {
            return AddGroup(new Group(name));
        }

        public Command AddCommand(Command cmd) {
            return Commands[cmd.Name] = cmd;
        }

        public Command AddCommand(string name, Func<List<string>, int?, Command, string> callback) {
            return Commands[name] = new Command(name, callback);
        }

        public Command AddCommand(string name, Func<List<string>, int?, string> callback) {
            return Commands[name] = new Command(name, callback);
        }

        public Command AddCommand(string name, Func<List<string>, string> callback) {
            return Commands[name] = new Command(name, callback);
        }
    }
}
