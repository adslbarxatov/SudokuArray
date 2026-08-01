using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму приложения
	/// </summary>
	public partial class SudokuArrayForm: Form
		{
		// Переменные и константы
		private const int buttonSize = 30;
		private List<Button> buttons = [];
		private Button newGameButton, checkButton, clearButton;
		private Button freeDigitsTip;

		/// <summary>
		/// Конструктор. Настраивает главную форму приложения
		/// </summary>
		public SudokuArrayForm ()
			{
			// Инициализация
			InitializeComponent ();
			RDGenerics.LoadWindowDimensions (this);

			this.Text = RDGenerics.DefaultAssemblyVisibleName;

			// Формирование поля
			for (int r = 0; r < SudokuArrayMath.SideSize; r++)
				{
				for (int c = 0; c < SudokuArrayMath.SideSize; c++)
					{
					Button lb = new Button ();

					SudokuArrayMath.SetProperty (lb, PropertyTypes.EmptyValue);
					SudokuArrayMath.SetProperty (lb, PropertyTypes.OldColor);
					lb.TextAlign = ContentAlignment.MiddleCenter;
					lb.Width = lb.Height = buttonSize;
					lb.Left = lb.Width * (c + 1) + 3 * (c / SudokuArrayMath.SquareSize -
						SudokuArrayMath.SquareSize / 2);
					lb.Top = lb.Height * (r + 1) + 3 * (r / SudokuArrayMath.SquareSize -
						SudokuArrayMath.SquareSize / 2) + buttonSize;
					lb.Cursor = Cursors.UpArrow;
					lb.KeyDown += Lb_KeyDown;
					lb.FlatStyle = FlatStyle.Popup;
					lb.MouseWheel += Lb_MouseWheel;
					lb.MouseDown += Lb_MouseClick;
					lb.MouseHover += Lb_MouseHover;

					this.Controls.Add (lb);
					buttons.Add (lb);
					}
				}

			// Формирование вспомогательных кнопок
			newGameButton = new Button ();
			newGameButton.Name = "NewGameButton";
			checkButton = new Button ();
			checkButton.Name = "CheckButton";
			clearButton = new Button ();
			clearButton.Name = "ClearButton";
			freeDigitsTip = new Button ();

			LocalizeForm ();

			newGameButton.TextAlign = checkButton.TextAlign = clearButton.TextAlign = freeDigitsTip.TextAlign =
				ContentAlignment.MiddleCenter;
			newGameButton.Width = checkButton.Width = clearButton.Width = 3 * buttonSize;
			newGameButton.Height = checkButton.Height = clearButton.Height = buttonSize;

			newGameButton.Left = buttonSize - 3;
			checkButton.Left = 4 * buttonSize;
			clearButton.Left = 7 * buttonSize + 3;

			freeDigitsTip.AutoSize = false;
			freeDigitsTip.FlatStyle = FlatStyle.Flat;
			freeDigitsTip.FlatAppearance.BorderSize = 0;
			freeDigitsTip.Top = buttons[buttons.Count - 1].Top + 19 * buttonSize / 16;
			freeDigitsTip.Left = buttons[0].Left;
			freeDigitsTip.Width = buttons[buttons.Count - 1].Left - buttons[0].Left + buttons[0].Width;
			freeDigitsTip.Height = 2 * buttons[buttons.Count - 1].Height / 3;
			freeDigitsTip.Click += FreeDigitsTip_Click;

			newGameButton.Top = checkButton.Top = clearButton.Top =
				buttons[buttons.Count - 1].Top + 2 * buttonSize;
			newGameButton.FlatStyle = checkButton.FlatStyle = clearButton.FlatStyle = FlatStyle.Flat;
			newGameButton.Font = checkButton.Font = clearButton.Font = MainMenu.Font;
			freeDigitsTip.Font = new Font (this.Font.FontFamily, 7 * freeDigitsTip.Font.Size / 8);

			newGameButton.Click += NewGame_Click;
			checkButton.Click += MCheck_Click;
			clearButton.Click += MClear_Click;

			this.Controls.Add (newGameButton);
			this.Controls.Add (checkButton);
			this.Controls.Add (clearButton);
			this.Controls.Add (freeDigitsTip);

			// Загрузка настроек
			MParameters_Clicked (null, null);
			}

		// Подсветка простреливаемых ячеек
		private void Lb_MouseHover (object sender, EventArgs e)
			{
			uint idx = (uint)buttons.IndexOf ((Button)sender);

			// Обновление цветов
			if (SudokuArrayMath.HighlightType != HighlightTypes.None)
				{
				bool squaresToo = (SudokuArrayMath.HighlightType == HighlightTypes.LinesAndSquares);
				for (int i = 0; i < buttons.Count; i++)
					{
					if (i == idx)
						SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.SelectedCell);
					else if (SudokuArrayMath.IsCellAffected (idx, (uint)i, squaresToo))
						SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.AffectedCell);
					else
						SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.DeselectedCell);
					}
				}

			// Обновление подсказки
			if (SudokuArrayMath.ShowFreeDigitsFlag)
				{
				if (!SudokuArrayMath.CheckCondition (buttons[(int)idx], ConditionTypes.IsEmpty))
					{
					freeDigitsTip.Text = "";
					return;
					}

				string existing = "";
				for (int i = 0; i < buttons.Count; i++)
					{
					if (SudokuArrayMath.IsCellAffected (idx, (uint)i, true))
						if (!existing.Contains (buttons[i].Text))
							existing += buttons[i].Text;
					}

				freeDigitsTip.Text = SudokuArrayMath.GetFreeDigitsForCell (existing);
				}
			}

		// Метод локализует форму
		private void LocalizeForm ()
			{
			// Меню
			RDLocale.SetControlText (MActivities);
			RDLocale.SetControlText (MSettings);
			RDLocale.SetControlText (MInfo);

			for (int i = 0; i < MActivities.DropDownItems.Count; i++)
				RDLocale.SetControlText (MActivities.Name, MActivities.DropDownItems[i]);

			for (int i = 0; i < MGenerate.DropDownItems.Count; i++)
				RDLocale.SetControlText (MGenerate.Name, MGenerate.DropDownItems[i]);

			for (int i = 0; i < MSettings.DropDownItems.Count; i++)
				RDLocale.SetControlText (MSettings.Name, MSettings.DropDownItems[i]);

			for (int i = 0; i < MInfo.DropDownItems.Count; i++)
				RDLocale.SetControlText (MInfo.Name, MInfo.DropDownItems[i]);

			RDLocale.SetDefaultControlText (MExit, RDLDefaultTexts.Button_Exit);

			// Диалоги
			OFDialog.Title = RDLocale.GetText ("OFName");
			SFDialog.Title = RDLocale.GetText ("SFName");
			OFDialog.Filter = SFDialog.Filter = RDLocale.GetText ("OFFilter");

			OFPDialog.Title = RDLocale.GetText ("OFPName");
			SFPDialog.Title = RDLocale.GetText ("SFName");
			OFPDialog.Filter = SFPDialog.Filter = RDLocale.GetText ("OFPFilter");

			// Вспомогательные кнопки
			RDLocale.SetControlText (newGameButton);
			RDLocale.SetControlText (checkButton);
			RDLocale.SetControlText (clearButton);
			}

		/// <summary>
		/// Метод переопределяет обработку клавиатуры формой
		/// </summary>
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
			{
			switch (keyData)
				{
				// Перенаправление движения по кнопкам
				case Keys.Up:
					for (int i = 0; i < SudokuArrayMath.SideSize; i++)
						this.SelectNextControl (this.ActiveControl, false, true, false, true);
					return true;

				case Keys.Down:
					for (int i = 0; i < SudokuArrayMath.SideSize; i++)
						this.SelectNextControl (this.ActiveControl, true, true, false, true);
					return true;

				case Keys.Left:
					if (buttons.IndexOf ((Button)this.ActiveControl) % SudokuArrayMath.SideSize == 0)
						{
						for (int i = 1; i < SudokuArrayMath.SideSize; i++)
							this.SelectNextControl (this.ActiveControl, true, true, false, true);
						}
					else
						{
						this.SelectNextControl (this.ActiveControl, false, true, false, true);
						}
					return true;

				case Keys.Right:
					if ((buttons.IndexOf ((Button)this.ActiveControl) + 1) % SudokuArrayMath.SideSize == 0)
						{
						for (int i = 1; i < SudokuArrayMath.SideSize; i++)
							this.SelectNextControl (this.ActiveControl, false, true, false, true);
						}
					else
						{
						this.SelectNextControl (this.ActiveControl, true, true, false, true);
						}
					return true;

				// Остальные клавиши обрабатываются стандартной процедурой
				default:
					return base.ProcessCmdKey (ref msg, keyData);
				}
			}

		// Действия из меню программы

		// Решение из текущего состояния
		private void MSolve_Click (object sender, EventArgs e)
			{
			if (!Solve (true))
				RDInterface.LocalizedMessageBox (RDMessageFlags.Error | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
					"SolutionIsIncorrect", 1000);
			}

		// Проверка решения в текущем состоянии
		private void MCheck_Click (object sender, EventArgs e)
			{
			if (!Solve (false))
				ApplyPenalty ();
			}

		// Сброс решения (без очистки всех полей)
		private void MClear_Click (object sender, EventArgs e)
			{
			bool game = (SudokuArrayMath.GameMode != MatrixDifficulty.None);

			for (int i = 0; i < buttons.Count; i++)
				{
				if (SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.ContainsFoundValue) ||
					game && SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.ContainsNewValue))
					{
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.EmptyValue);
					}
				}
			}

		// Полный сброс
		private void MReset_Click (object sender, EventArgs e)
			{
			if (RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
				"ResetWarning",
				RDLDefaultTexts.Button_YesNoFocus, RDLDefaultTexts.Button_No) !=
				RDMessageButtons.ButtonOne)
				return;

			for (int i = 0; i < buttons.Count; i++)
				SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.EmptyValue);
			SudokuArrayMath.GameMode = MatrixDifficulty.None;
			}

		// Справка
		private void MHelp_Click (object sender, EventArgs e)
			{
			RDInterface.ShowAbout (false);
			}

		// Язык интерфейса
		private void MLanguage_Click (object sender, EventArgs e)
			{
			if (RDInterface.MessageBox ())
				LocalizeForm ();
			}

		// Закрытие окна
		private void MExit_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Загрузка таблицы из файла
		private void MLoad_Click (object sender, EventArgs e)
			{
			OFDialog.ShowDialog ();
			}

		private void OFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Попытка считывания файла
			string file;
			try
				{
				file = File.ReadAllText (OFDialog.FileName, RDGenerics.GetEncoding (RDEncodings.UTF8));
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_LoadFailure_Fmt),
					OFDialog.FileName));
				return;
				}

			// Обработка
			string line = SudokuArrayMath.ParseMatrixFromFile (file);
			if (string.IsNullOrWhiteSpace (line))
				{
				RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"MessageNotEnough");
				return;
				}

			// Загрузка
			for (int i = 0; i < buttons.Count; i++)
				{
				buttons[i].Text = SudokuArrayMath.GetAppearance (line[i].ToString ());
				if (SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.IsEmpty))
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.NewColor);
				else
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.OldColor);
				}

			// Сброс игрового режима
			SudokuArrayMath.GameMode = MatrixDifficulty.None;
			}

		// Выгрузка таблицы в файл
		private void MSave_Click (object sender, EventArgs e)
			{
			SFDialog.ShowDialog ();
			}

		private void SFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Выгрузка данных
			FlushMatrix ();
			string file = SudokuArrayMath.BuildMatrixToSave (SudokuArrayMath.SudokuField);

			// Сохранение
			try
				{
				File.WriteAllText (SFDialog.FileName, file, RDGenerics.GetEncoding (RDEncodings.UTF8));
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					SFDialog.FileName));
				return;
				}
			}

		// Выбор поля ввода
		private void Lb_KeyDown (object sender, KeyEventArgs e)
			{
			Button lb = (Button)sender;

			// В игровом режиме изменение проверенных ячеек запрещено
			if ((SudokuArrayMath.GameMode != MatrixDifficulty.None) &&
				!SudokuArrayMath.CheckCondition (lb, ConditionTypes.IsEmpty) &&
				!SudokuArrayMath.CheckCondition (lb, ConditionTypes.ContainsNewValue))
				return;

			switch (e.KeyCode)
				{
				case Keys.D1:
				case Keys.D2:
				case Keys.D3:
				case Keys.D4:
				case Keys.D5:
				case Keys.D6:
				case Keys.D7:
				case Keys.D8:
				case Keys.D9:
					SudokuArrayMath.SetProperty (lb, PropertyTypes.NewColor);
					lb.Text = SudokuArrayMath.GetAppearance ((Byte)(e.KeyCode - 48));
					break;

				default:
					SudokuArrayMath.SetProperty (lb, PropertyTypes.EmptyValue);
					break;
				}
			}

		// Нажание кнопок и прокрутка
		private void Lb_MouseWheel (object sender, MouseEventArgs e)
			{
			Lb_MouseClick (sender, e);
			}

		private void Lb_MouseClick (object sender, MouseEventArgs e)
			{
			Button b = (Button)sender;

			// В игровом режиме изменение проверенных ячеек запрещено
			if ((SudokuArrayMath.GameMode != MatrixDifficulty.None) &&
				!SudokuArrayMath.CheckCondition (b, ConditionTypes.IsEmpty) &&
				!SudokuArrayMath.CheckCondition (b, ConditionTypes.ContainsNewValue))
				return;

			int v = -1;
			bool plus;
			if ((e.Delta < 0) || (e.Button == MouseButtons.Right))
				plus = false;
			else
				plus = true;

			try
				{
				v = SudokuArrayMath.GetDigit (b.Text);
				v += (plus ? 1 : -1);
				}
			catch { }

			if (plus)
				{
				if (v < 0)
					b.Text = SudokuArrayMath.GetAppearance (1);
				else if (v > 9)
					SudokuArrayMath.SetProperty (b, PropertyTypes.EmptyValue);
				else
					b.Text = SudokuArrayMath.GetAppearance ((Byte)v);
				}
			else
				{
				if (v < 0)
					b.Text = SudokuArrayMath.GetAppearance (9);
				else if (v < 1)
					SudokuArrayMath.SetProperty (b, PropertyTypes.EmptyValue);
				else
					b.Text = SudokuArrayMath.GetAppearance ((Byte)v);
				}

			SudokuArrayMath.SetProperty (b, PropertyTypes.NewColor);
			}

		// Решение задачи
		private bool Solve (bool LoadResults)
			{
			// Сборка массива
			Byte[,] matrix = new Byte[SudokuArrayMath.SideSize, SudokuArrayMath.SideSize];
			for (int r = 0; r < SudokuArrayMath.SideSize; r++)
				{
				for (int c = 0; c < SudokuArrayMath.SideSize; c++)
					{
					Button ct = buttons[r * (int)SudokuArrayMath.SideSize + c];
					if (!SudokuArrayMath.CheckCondition (ct, ConditionTypes.IsEmpty) &&
						!SudokuArrayMath.CheckCondition (ct, ConditionTypes.ContainsFoundValue))
						matrix[r, c] = SudokuArrayMath.GetDigit (ct.Text);
					else
						matrix[r, c] = 0;
					}
				}

			// Инициализация задачи
			SudokuArrayMath.InitializeSolution (matrix);
			switch (SudokuArrayMath.CurrentStatus)
				{
				case SolutionResults.InitialMatrixIsInvalid:
					throw new Exception ("Invalid initialization of the solution, debug is required");

				case SolutionResults.InitialMatrixIsUnsolvable:
					if (LoadResults)
						for (int i = 0; i < buttons.Count; i++)
							SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.ErrorColor);

					return false;
				}

			// Решение задачи
			SudokuArrayMath.FindSolution ();
			switch (SudokuArrayMath.CurrentStatus)
				{
				case SolutionResults.NoSolutionsFound:
				case SolutionResults.NotInited:
					if (LoadResults)
						for (int i = 0; i < buttons.Count; i++)
							SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.ErrorColor);

					return false;

				// Не перекрашивать поле
				case SolutionResults.SearchAborted:
					// Не считать нарушением правил
					return true;
				}

			// Игровой режим
			if (!LoadResults)
				{
				// Цвет новых ячеек меняется на фоновый
				uint newCellsCount = 0, emptyCellsCount = 0;
				bool gameMode = (SudokuArrayMath.GameMode != MatrixDifficulty.None);

				for (int i = 0; i < buttons.Count; i++)
					{
					if (gameMode)
						{
						if (SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.IsEmpty))
							emptyCellsCount++;
						else if (SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.ContainsNewValue))
							newCellsCount++;
						}

					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.OldColor);
					}

				// Контроль матрицы на неизменность
				if (gameMode)
					{
					// Расчёт очков
					uint score = SudokuArrayMath.GetScore (newCellsCount);
					bool win = (emptyCellsCount < 2);
					if (win)
						score += SudokuArrayMath.GetScore (ScoreTypes.GameCompletion);

					// Отображение сведений о достижениях (обязательно до обновления очков)
					string achiLine = "";
					for (StoredFields i = StoredFields.Achi_OneOrLess_Easy; i <= StoredFields.Achi_OneMove_Hard; i++)
						{
						if (!win)
							break;

						if (!SudokuArrayMath.CheckAchievement (i))
							continue;

						string achiText = SudokuArrayMath.GetAchievementDescription (i);
						int left = achiText.IndexOf (RDLocale.RN);
						achiLine += (" " + achiText.Substring (0, left));

						uint tip = 1u << (int)i;
						if ((RDGenerics.TipsState & tip) == 0)
							{
							RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText |
								RDMessageFlags.LockSmallSize, achiText);
							RDGenerics.TipsState |= tip;
							}
						}

					// Обновление счёта
					SudokuArrayMath.UpdateGameScore (false, score);

					// Отображение результата и отключение игрового режима до следующей генерации
					string msgText = "";
					uint msgTimeout = 3000;
					if (win && !SudokuArrayMath.ShowStatsOnWinFlag)
						{
						msgText += (RDLocale.GetText ("SolvedText") + RDLocale.RNRN);
						}
					else if (!win)
						{
						msgText += (RDLocale.GetText ("SolutionIsCorrect") + RDLocale.RNRN);
						}
					else
						{
						msgTimeout = 2000;
						}

					msgText += ("+" + score.ToString () + SudokuArrayMath.ScoreChar);
					if (!string.IsNullOrWhiteSpace (achiLine))
						msgText += "\t+" + achiLine;

					RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
						msgText, msgTimeout);

					// Отобразить решение в случае выигрыша (без return; режим игры отключается далее)
					if (win)
						{
						if (SudokuArrayMath.ShowStatsOnWinFlag)
							MStats_Click (null, null);
						}

					// Иначе продолжить игру
					else
						{
						return true;
						}
					}

				// Не отображать решение вне игрового режима
				else
					{
					RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
						RDLocale.GetText ("SolutionIsCorrect"), 1000);
					return true;
					}
				}

			// Отображение решения
			SudokuArrayMath.GameMode = MatrixDifficulty.None;
			for (int r = 0; r < SudokuArrayMath.SideSize; r++)
				{
				for (int c = 0; c < SudokuArrayMath.SideSize; c++)
					{
					Button ct = buttons[r * (int)SudokuArrayMath.SideSize + c];
					if (!SudokuArrayMath.CheckCondition (ct, ConditionTypes.IsEmpty) &&
						!SudokuArrayMath.CheckCondition (ct, ConditionTypes.ContainsFoundValue))
						{
						SudokuArrayMath.SetProperty (ct, PropertyTypes.OldColor);
						}
					else
						{
						ct.Text = SudokuArrayMath.GetAppearance (SudokuArrayMath.ResultMatrix[r, c]);
						SudokuArrayMath.SetProperty (ct, PropertyTypes.SuccessColor);
						}
					}
				}

			// Выполнено
			return true;
			}

		// Закрытие окна
		private void SudokuArrayForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Отмена текущего решения
			MClear_Click (null, null);

			// Сохранение поля судоку
			FlushMatrix ();

			// Сохранение окна
			RDGenerics.SaveWindowDimensions (this);
			}

		// Сохранение текущей матрицы
		private void FlushMatrix ()
			{
			string sudoku = "";
			for (int i = 0; i < buttons.Count; i++)
				sudoku += SudokuArrayMath.GetDigit (buttons[i].Text).ToString ();

			SudokuArrayMath.SudokuField = sudoku;
			}

		// Генерация матрицы судоку
		private void MGenerate_Click (object sender, EventArgs e)
			{
			// Контроль
			if ((e != null) && (SudokuArrayMath.GameMode != MatrixDifficulty.None))
				{
				if (RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					"GameIsNotCompletedMessage", RDLDefaultTexts.Button_YesNoFocus, RDLDefaultTexts.Button_No) !=
					RDMessageButtons.ButtonOne)
					return;
				}

			// Запуск
			ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
			string n = tsmi.Name.Substring (tsmi.Name.Length - 1);
			MatrixDifficulty diff = (MatrixDifficulty)uint.Parse (n);
			SudokuArrayMath.SetGenerationDifficulty (diff);

			SudokuArrayMath.GenerateMatrix ();

			// Отображение результата
			for (int r = 0; r < SudokuArrayMath.SideSize; r++)
				{
				for (int c = 0; c < SudokuArrayMath.SideSize; c++)
					{
					Button ct = buttons[r * SudokuArrayMath.SideSize + c];
					if (SudokuArrayMath.ResultMatrix[r, c] == 0)
						SudokuArrayMath.SetProperty (ct, PropertyTypes.EmptyValue);
					else
						ct.Text = SudokuArrayMath.GetAppearance (SudokuArrayMath.ResultMatrix[r, c]);
					SudokuArrayMath.SetProperty (ct, PropertyTypes.OldColor);
					}
				}

			// Взведение игрового режима
			SudokuArrayMath.GameMode = diff;

			// Завершено
			}

		// Метод применяет штраф
		private static void ApplyPenalty ()
			{
			uint score = SudokuArrayMath.GetScore (ScoreTypes.Penalty);
			SudokuArrayMath.UpdateGameScore (true, score);

			string text = RDLocale.GetText ("SolutionIsIncorrect");
			if (SudokuArrayMath.GameMode != MatrixDifficulty.None)
				{
				text += (RDLocale.RNRN + "–" + score.ToString () + SudokuArrayMath.ScoreChar);
				RDInterface.MessageBox (RDMessageFlags.Error | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
					text, 1500);
				}
			else
				{
				RDInterface.MessageBox (RDMessageFlags.Error | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
					text, 1000);
				}
			}

		// Метод отображает игровую статистику
		private void MStats_Click (object sender, EventArgs e)
			{
			_ = new SudokuArrayResults (sender == null);
			}

		// Запуск новой игры из интерфейсной кнопки
		private void NewGame_Click (object sender, EventArgs e)
			{
			// Контроль
			if (SudokuArrayMath.GameMode != MatrixDifficulty.None)
				{
				if (RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					"GameIsNotCompletedMessage", RDLDefaultTexts.Button_YesNoFocus, RDLDefaultTexts.Button_No) !=
					RDMessageButtons.ButtonOne)
					return;
				}

			RDMessageButtons res = RDInterface.MessageBox (RDMessageFlags.Question |
				RDMessageFlags.CenterText | RDMessageFlags.NoSound,
				RDLocale.GetText ("DifficultyMessage"), RDLocale.GetText ("MGenerate_MDifficulty0"),
				RDLocale.GetText ("MGenerate_MDifficulty1"), RDLocale.GetText ("MGenerate_MDifficulty2"));

			MGenerate_Click (MGenerate.DropDownItems[(int)res - 1], null);
			}

		// Отдельное окно настроек
		private void MParameters_Clicked (object sender, EventArgs e)
			{
			// Запрос настроек
			if (sender != null)
				{
				FlushMatrix ();
				_ = new SudokuArraySettings ();
				}

			string line = SudokuArrayMath.SudokuField;

			// Адаптация к игровому режиму
			/*bool game = (SudokuArrayMath.AppMode == AppModes.Game);*/
			bool game = SudokuArrayMath.AppModeIsGame;

			this.ClientSize = new Size ((int)(SudokuArrayMath.SideSize + 2) * buttonSize,
				(int)(SudokuArrayMath.SideSize + 2) * buttonSize + (game ? 3 : 1) * buttonSize);
			newGameButton.Visible = checkButton.Visible = clearButton.Visible = game;
			freeDigitsTip.Visible = game && SudokuArrayMath.ShowFreeDigitsFlag;

			if (!game)
				SudokuArrayMath.GameMode = MatrixDifficulty.None;

			// Цветовая схема
			this.BackColor = SudokuArrayMath.BackgroundColor;
			for (int i = 0; i < buttons.Count; i++)
				{
				SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.DeselectedCell);

				// Переназначение цветов для дальнейшей корректной работы метода CheckCondition
				if (SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.ContainsFoundValue))
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.SuccessColor);
				else if (SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.ContainsNewValue))
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.NewColor);
				else if (SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.ContainsErrorValue))
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.ErrorColor);
				else
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.OldColor);
				}

			newGameButton.ForeColor = checkButton.ForeColor = clearButton.ForeColor = freeDigitsTip.ForeColor =
				buttons[0].ForeColor;

			// Представление ячеек
			for (int i = 0; i < buttons.Count; i++)
				{
				buttons[i].Text = SudokuArrayMath.GetAppearance (line[i].ToString ());

				if ((sender == null) && !SudokuArrayMath.CheckCondition (buttons[i], ConditionTypes.IsEmpty))
					SudokuArrayMath.SetProperty (buttons[i], PropertyTypes.OldColor);
				}
			}

		// Перенос выигрышей
		private void MLoadProfile_Click (object sender, EventArgs e)
			{
			/*RDMessageButtons res = RDInterface.LocalizedMessageBox (RDMessageFlags.Question | RDMessageFlags.LeftText,
				"ScoresExchangeMessage", RDLDefaultTexts.Button_Copy, RDLDefaultTexts.Button_Load, RDLDefaultTexts.Button_Cancel);

			switch (res)
				{
				case RDMessageButtons.ButtonOne:
					RDGenerics.SendToClipboard (SudokuArrayMath.GetPortableScoresLine (), true);
					break;

				case RDMessageButtons.ButtonTwo:
					if (SudokuArrayMath.SetPortableScoresLine (RDGenerics.GetFromClipboard ()))
						MStats_Click (sender, null);
					else
						RDInterface.LocalizedMessageBox (RDMessageFlags.Error | RDMessageFlags.CenterText,
							"ScoresExchangeError", 1000);
					break;
				}*/
			OFPDialog.ShowDialog ();
			}

		private void OFPDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Попытка считывания файла
			byte[] file;
			try
				{
				file = File.ReadAllBytes (OFPDialog.FileName);
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_LoadFailure_Fmt),
					OFPDialog.FileName));
				return;
				}

			// Обработка
			string line = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (file);
			if (SudokuArrayMath.ParseExchangeFile (line))
				MStats_Click (sender, null);
			else
				RDInterface.LocalizedMessageBox (RDMessageFlags.Error | RDMessageFlags.CenterText,
					"ScoresExchangeError", 1000);
			}

		private void MSaveProfile_Click (object sender, EventArgs e)
			{
			SFPDialog.ShowDialog ();
			}

		private void SFPDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Попытка записи
			string line = SudokuArrayMath.BuildExchangeFile ();
			byte[] file = RDGenerics.GetEncoding (RDEncodings.UTF8).GetBytes (line);
			try
				{
				File.WriteAllBytes (SFPDialog.FileName, file);
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					SFPDialog.FileName));
				return;
				}
			}

		// Отображение подсказки к доступным цифрам
		private void FreeDigitsTip_Click (object sender, EventArgs e)
			{
			if (string.IsNullOrWhiteSpace (freeDigitsTip.Text) ||
				string.IsNullOrWhiteSpace (SudokuArrayMath.LastFreeDigitsDescription))
				return;

			RDMessageFlags f = RDMessageFlags.CenterText | RDMessageFlags.NoSound | RDMessageFlags.LockSmallSize;
			if (freeDigitsTip.Text.StartsWith ('!'))
				f |= RDMessageFlags.Error;
			else if (freeDigitsTip.Text.StartsWith ('–'))
				f |= RDMessageFlags.Success;
			else
				f |= RDMessageFlags.Warning;

			RDInterface.MessageBox (f, SudokuArrayMath.LastFreeDigitsDescription, 1500);
			}
		}
	}
