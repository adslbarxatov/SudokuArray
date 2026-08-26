using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает функционал приложения
	/// </summary>
	public partial class App: Application
		{
		#region Общие переменные и константы

		// Параметры запуска приложения
		private RDAppStartupFlags flags;

		// Цветовая схема
		private Color aboutMasterBackColor = Color.FromArgb ("#F0FFF0");
		private Color aboutFieldBackColor = Color.FromArgb ("#D0FFD0");
		private Color settingsMasterBackColor = Color.FromArgb ("#FFFFF0");
		private Color settingsFieldBackColor = Color.FromArgb ("#FFFFD0");
		private Color resultsMasterBackColor = Color.FromArgb ("#FFFFF0");
		private Color resultsFieldBackColor = Color.FromArgb ("#FFFFD0");
		private Color stubColor = RDInterface.GetInterfaceColor (RDInterfaceColors.MediumGrey);

		// Контекстные меню
		private List<string> difficultyVariants = [];
		private List<string> colorSchemeVariants = [];
		private List<List<string>> menuVariants = [];
		private List<string> appearanceVariants = [];
		private List<string> highlightVariants = [];
		private static List<string> scoresExchangeVariants = [];

		// Номер текущей выбранной кнопки
		private int currentButtonIndex = -1;

		#endregion

		#region Переменные страниц

		private ContentPage solutionPage, aboutPage, settingsPage, resultsPage;

		private Label aboutFontSizeField, solutionTipLabel;

		private Button languageButton, solutionButton, checkButton, generateButton,
			clearButton, menuButton, colorSchemeButton, cellsAppearanceButton,
			highlightButton, freeDigitsTipButton, scoreField, achievementField;
		private List<Button> numberButtons = [];
		private List<Button> inputButtons = [];

		private StackLayout masterField;
		private StackLayout numbersField = [];
		private StackLayout inputField = [];

		private Switch gameModeSwitch, keepScreenOnSwitch, showFreeDigitsSwitch, showStatsOnWinSwitch;

		#endregion

		#region Запуск и настройка

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App ()
			{
			// Инициализация
			InitializeComponent ();
			}

		// Замена определению MainPage = new MasterPage ()
		protected override Window CreateWindow (IActivationState activationState)
			{
			return new Window (AppShell ());
			}

		// Инициализация разметки страниц
		private Page AppShell ()
			{
			Page mainPage = new MasterPage ();
			flags = RDGenerics.GetAppStartupFlags (RDAppStartupFlags.DisableXPUN | RDAppStartupFlags.CanReadFiles |
				RDAppStartupFlags.CanWriteFiles);

			// Общая конструкция страниц приложения
			solutionPage = RDInterface.ApplyPageSettings (new SolutionPage (),
				RDLocale.GetText ("SolutionPage"), stubColor);
			settingsPage = RDInterface.ApplyPageSettings (new SettingsPage (),
				RDLocale.GetText ("SettingsPage"), settingsMasterBackColor);
			aboutPage = RDInterface.ApplyPageSettings (new AboutPage (),
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				aboutMasterBackColor);
			resultsPage = RDInterface.ApplyPageSettings (new ResultsPage (),
				RDLocale.GetText ("ResultsPage"), resultsMasterBackColor);

			RDInterface.SetMasterPage (mainPage, solutionPage, stubColor);

			#region Основная страница

			// Ориентация экрана
			DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;
			masterField = (StackLayout)solutionPage.FindByName ("MasterField");

			numbersField.HorizontalOptions = numbersField.VerticalOptions = LayoutOptions.Center;
			numbersField.Orientation = StackOrientation.Vertical;

			// Сборка поля ввода матрицы
			List<StackLayout> numbersSL = [];

			for (int i = 0; i < SudokuArrayMath.FullSize; i++)
				{
				// Добавление горизонтальных пробелов
				if ((i != 0) && (i % (SudokuArrayMath.SquareSize * SudokuArrayMath.SideSize) == 0))
					{
					Label l = new Label ();
					l.WidthRequest = l.HeightRequest = 5;
					numbersField.Add (l);
					}

				// Добавление строковых полей и вертикальных пробелов
				if ((i % SudokuArrayMath.SideSize) == 0)
					{
					StackLayout sl = [];
					sl.Orientation = StackOrientation.Horizontal;
					sl.HorizontalOptions = LayoutOptions.Center;
					numbersSL.Add (sl);
					numbersField.Add (sl);
					}
				else if ((i % SudokuArrayMath.SquareSize) == 0)
					{
					Label l = new Label ();
					l.WidthRequest = l.HeightRequest = 5;
					numbersSL[numbersSL.Count - 1].Add (l);
					}

				// Добавление кнопок
				Button b = new Button ();
				RDInterface.ApplyButtonDefaults (b, true);

				b.FontFamily = RDGenerics.MonospaceFont;
				SudokuArrayMath.SetProperty (b, PropertyTypes.OldColor);
				b.WidthRequest = b.HeightRequest = RDInterface.MasterFontSize * 2.25;
				b.Padding = Thickness.Zero;
				b.Margin = new Thickness (1);
				SudokuArrayMath.SetProperty (b, PropertyTypes.EmptyValue);
				b.Clicked += SelectCurrentButton;
				if (RDGenerics.IsTV)
					b.Focused += FocusButton;

				numberButtons.Add (b);
				numbersSL[numbersSL.Count - 1].Add (b);
				}

			masterField.Add (numbersField);

			// Разделитель
			Label sp = new Label ();
			sp.WidthRequest = sp.HeightRequest = 15;
			masterField.Add (sp);

			// Сборка вспомогательной клавиатуры
			inputField.HorizontalOptions = inputField.VerticalOptions = LayoutOptions.Center;
			inputField.Orientation = StackOrientation.Vertical;

			List<StackLayout> inputSL = [];

			for (int i = 9; i >= 0; i--)
				{
				// Добавление строковых полей
				if (i % 3 == 0)
					{
					StackLayout sl = [];
					sl.Orientation = StackOrientation.Horizontal;
					sl.HorizontalOptions = LayoutOptions.Center;
					inputSL.Add (sl);
					inputField.Add (sl);
					sl.IsVisible = !RDGenerics.IsTV;
					}

				Button b = new Button ();
				RDInterface.ApplyButtonDefaults (b, true);

				b.FontFamily = RDGenerics.MonospaceFont;
				b.Padding = Thickness.Zero;
				b.Margin = new Thickness (3);
				b.Clicked += SetValueForCurrentButton;

				b.WidthRequest = b.HeightRequest = RDInterface.MasterFontSize * 2.75;
				if (i == 0)
					b.Text = " ";

				if (inputButtons.Count > 0)
					inputButtons.Insert (0, b);
				else
					inputButtons.Add (b);

				if (inputSL[inputSL.Count - 1].Count > 0)
					inputSL[inputSL.Count - 1].Insert (0, b);
				else
					inputSL[inputSL.Count - 1].Add (b);
				}

			// Разделитель
			if (!RDGenerics.IsTV)
				{
				Label msp = new Label ();
				msp.WidthRequest = msp.HeightRequest = 10;
				inputField.Add (msp);
				}

			StackLayout msl = [];
			msl.Orientation = RDGenerics.IsTV ? StackOrientation.Vertical : StackOrientation.Horizontal;
			msl.HorizontalOptions = LayoutOptions.Center;
			inputSL.Add (msl);
			inputField.Add (msl);

			// Добавление управляющих кнопок
			freeDigitsTipButton = RDInterface.ApplyButtonSettings (solutionPage, null, RDDefaultButtons.Menu,
				stubColor, FreeDigitsTip_Click, true);
			freeDigitsTipButton.Text = "";
			freeDigitsTipButton.FontFamily = RDGenerics.MonospaceFont;
			freeDigitsTipButton.FontSize /= 1.75;
			inputSL[inputSL.Count - 1].Add (freeDigitsTipButton);

			generateButton = RDInterface.ApplyButtonSettings (solutionPage, null, RDDefaultButtons.Menu,
				stubColor, GenerateMatrix_Clicked, true);
			generateButton.Text = "✨";
			inputSL[inputSL.Count - 1].Add (generateButton);

			checkButton = RDInterface.ApplyButtonSettings (solutionPage, null, RDDefaultButtons.Menu,
				stubColor, CheckSolution_Clicked, true);
			checkButton.Text = "☑️";

			if (RDGenerics.IsTV)
				checkButton.HeightRequest *= 3;
			else
				checkButton.WidthRequest *= 2;

			inputSL[inputSL.Count - 1].Add (checkButton);

			clearButton = RDInterface.ApplyButtonSettings (solutionPage, null, RDDefaultButtons.Menu,
				stubColor, ClearSolution_Clicked, true);
			clearButton.Text = "↩️";
			inputSL[inputSL.Count - 1].Add (clearButton);

			solutionButton = RDInterface.ApplyButtonSettings (solutionPage, null, RDDefaultButtons.Menu,
				stubColor, SolveSudoku_Clicked, true);
			solutionButton.Text = "✅";

			if (RDGenerics.IsTV)
				solutionButton.HeightRequest *= 3;
			else
				solutionButton.WidthRequest *= 2;

			inputSL[inputSL.Count - 1].Add (solutionButton);

			menuButton = RDInterface.ApplyButtonSettings (solutionPage, null, RDDefaultButtons.Menu,
				stubColor, MenuButton_Clicked, true);
			inputSL[inputSL.Count - 1].Add (menuButton);

			// Подсказка о выполнении решения
			solutionTipLabel = RDInterface.ApplyLabelSettings (solutionPage, null, RDLocale.GetText ("SolutionTip"),
				RDLabelTypes.TipCenter);
			inputField.Add (solutionTipLabel);

			masterField.Add (inputField);

			// Загрузка расположения клавиатуры
			SetInterfaceState (true);

			// Этот список нужен в нескольких местах
			difficultyVariants.Add (SudokuArrayMath.EasyPrefix + RDLocale.GetText ("Difficulty0"));
			difficultyVariants.Add (SudokuArrayMath.MediumPrefix + RDLocale.GetText ("Difficulty1"));
			difficultyVariants.Add (SudokuArrayMath.HardPrefix + RDLocale.GetText ("Difficulty2"));

			#endregion

			#region Страница настроек

			gameModeSwitch = RDInterface.ApplySwitchSettings (settingsPage, "GameModeSwitch", false,
				settingsFieldBackColor, GameModeSwitch_Toggled, SudokuArrayMath.AppModeIsGame);
			RDInterface.ApplyLabelSettings (settingsPage, "GameModeLabel", RDLocale.GetText ("GameModeLabel"),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyLabelSettings (settingsPage, "GameModeTip", RDLocale.GetText ("GameModeTip"),
				RDLabelTypes.TipJustify);
			GameModeSwitch_Toggled (null, null);

			keepScreenOnSwitch = RDInterface.ApplySwitchSettings (settingsPage, "KeepScreenOnSwitch", false,
				settingsFieldBackColor, KeepScreenOnSwitch_Toggled, RDInterface.KeepScreenOn);
			RDInterface.ApplyLabelSettings (settingsPage, "KeepScreenOnLabel", RDLocale.GetText ("KeepScreenOnLabel"),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyLabelSettings (settingsPage, "KeepScreenOnTip", RDLocale.GetText ("KeepScreenOnTip"),
				RDLabelTypes.TipJustify);

			showFreeDigitsSwitch = RDInterface.ApplySwitchSettings (settingsPage, "ShowFreeDigitsSwitch", false,
				settingsFieldBackColor, ShowFreeDigits_Toggled, SudokuArrayMath.ShowFreeDigitsFlag);
			RDInterface.ApplyLabelSettings (settingsPage, "ShowFreeDigitsLabel", RDLocale.GetText ("ShowFreeDigitsLabel"),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyLabelSettings (settingsPage, "ShowFreeDigitsTip", RDLocale.GetText ("ShowFreeDigitsTip"),
				RDLabelTypes.TipJustify);

			showStatsOnWinSwitch = RDInterface.ApplySwitchSettings (settingsPage, "ShowStatsOnWinSwitch", false,
				settingsFieldBackColor, ShowStatsOnWinSwitch_Toggled, SudokuArrayMath.ShowStatsOnWinFlag);
			RDInterface.ApplyLabelSettings (settingsPage, "ShowStatsOnWinLabel", RDLocale.GetText ("ShowStatsOnWinLabel"),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyLabelSettings (settingsPage, "ShowStatsOnWinTip", RDLocale.GetText ("ShowStatsOnWinTip"),
				RDLabelTypes.TipJustify);

			RDInterface.ApplyLabelSettings (settingsPage, "RestartTipLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Message_RestartRequired),
				RDLabelTypes.TipCenter);

			RDInterface.ApplyLabelSettings (settingsPage, "LanguageLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceLanguage) + ":",
				RDLabelTypes.DefaultLeft);
			languageButton = RDInterface.ApplyButtonSettings (settingsPage, "LanguageSelector",
				RDLocale.LanguagesNamesList[(int)RDLocale.CurrentLanguage],
				settingsFieldBackColor, SelectLanguage_Clicked);
			RDInterface.ApplyLabelSettings (settingsPage, "LanguageTip", RDLocale.GetText ("LanguageTip"),
				RDLabelTypes.TipJustify);

			RDInterface.ApplyLabelSettings (settingsPage, "FontSizeLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceFontSize),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyButtonSettings (settingsPage, "FontSizeInc",
				RDDefaultButtons.Increase, settingsFieldBackColor, FontSizeButton_Clicked, true);
			RDInterface.ApplyButtonSettings (settingsPage, "FontSizeDec",
				RDDefaultButtons.Decrease, settingsFieldBackColor, FontSizeButton_Clicked, true);
			aboutFontSizeField = RDInterface.ApplyLabelSettings (settingsPage, "FontSizeField",
				" ", RDLabelTypes.DefaultCenter);
			RDInterface.ApplyLabelSettings (settingsPage, "FontSizeTip", RDLocale.GetText ("FontSizeTip"),
				RDLabelTypes.TipJustify);

			highlightButton = RDInterface.ApplyButtonSettings (settingsPage, "HighlightAffectedButton", " ",
				settingsFieldBackColor, HighlightAffectedButton_Clicked);
			RDInterface.ApplyLabelSettings (settingsPage, "HighlightAffectedLabel",
				RDLocale.GetText ("HighlightAffectedLabel"), RDLabelTypes.DefaultLeft);
			RDInterface.ApplyLabelSettings (settingsPage, "HighlightAffectedTip",
				RDLocale.GetText ("HighlightAffectedTip"), RDLabelTypes.TipJustify);
			HighlightAffectedButton_Clicked (null, null);

			colorSchemeButton = RDInterface.ApplyButtonSettings (settingsPage, "ColorSchemeButton", " ",
				settingsFieldBackColor, ColorSchemeButton_Clicked);
			RDInterface.ApplyLabelSettings (settingsPage, "ColorSchemeLabel", RDLocale.GetText ("ColorSchemeLabel"),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyLabelSettings (settingsPage, "ColorSchemeTip", RDLocale.GetText ("ColorSchemeTip"),
				RDLabelTypes.TipJustify);
			ColorSchemeButton_Clicked (null, null);

			cellsAppearanceButton = RDInterface.ApplyButtonSettings (settingsPage, "CellsAppearanceButton", " ",
				settingsFieldBackColor, CellsAppearanceButton_Clicked);
			RDInterface.ApplyLabelSettings (settingsPage, "CellsAppearanceLabel", RDLocale.GetText ("CellsAppearanceLabel"),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyLabelSettings (settingsPage, "CellsAppearanceTip", RDLocale.GetText ("CellsAppearanceTip"),
				RDLabelTypes.TipJustify);
			CellsAppearanceButton_Clicked (null, null);

			#endregion

			#region Страница «О программе»

			RDInterface.ApplyLabelSettings (aboutPage, "AboutLabel",
				RDGenerics.AppAboutLabelText, RDLabelTypes.AppAbout);

			RDInterface.ApplyButtonSettings (aboutPage, "ManualsButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_ReferenceMaterials),
				aboutFieldBackColor, ReferenceButton_Click);

			Button hlp = RDInterface.ApplyButtonSettings (aboutPage, "HelpButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_HelpSupport),
				aboutFieldBackColor, HelpButton_Click);
			hlp.IsVisible = !RDGenerics.IsTV;

			Image qrImage = (Image)aboutPage.FindByName ("QRImage");
			qrImage.IsVisible = RDGenerics.IsTV;

			FontSizeButton_Clicked (null, null);

			#endregion

			#region Страница результатов

			scoreField = RDInterface.ApplyButtonSettings (resultsPage, "ScoreField", " ", resultsMasterBackColor,
				SendResultToClipboard, RDButtonFlags.BiggerFontSize);
			scoreField.FontFamily = RDGenerics.SerifFont;

			achievementField = RDInterface.ApplyButtonSettings (resultsPage, "AchievementField", " ", resultsMasterBackColor,
				SendResultToClipboard, RDButtonFlags.BiggerFontSize);
			achievementField.FontFamily = RDGenerics.SerifFont;

			RDInterface.ApplyButtonSettings (resultsPage, "LoadProfileButton", RDLocale.GetText ("ScoresExchangeLoad"),
				resultsFieldBackColor, LoadProfile_Clicked);
			RDInterface.ApplyButtonSettings (resultsPage, "SaveProfileButton", RDLocale.GetText ("ScoresExchangeSave"),
				resultsFieldBackColor, SaveProfile_Clicked);

			#endregion

			// Отображение подсказок первого старта
			ShowStartupTips ();
			return mainPage;
			}

		// Метод отображает подсказки при первом запуске
		private async void ShowStartupTips ()
			{
			// Контроль XPUN
			if (!flags.HasFlag (RDAppStartupFlags.DisableXPUN))
				await RDInterface.XPUNLoop ();

			// Требование принятия Политики
			await RDInterface.PolicyLoop ();

			// Приветствие
			if (!((TipTypes)RDGenerics.TipsState).HasFlag (TipTypes.WelcomeTip))
				{
				await RDInterface.ShowMessage (RDLocale.GetText ("WelcomeTip"),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				RDGenerics.TipsState |= (uint)TipTypes.WelcomeTip;
				}
			}

		// Изменение ориентации экрана
		private async void Current_MainDisplayInfoChanged (object sender, DisplayInfoChangedEventArgs e)
			{
			await Task.Delay (500);

			if (RDGenerics.IsTV)
				{
				masterField.Orientation = StackOrientation.Horizontal;
				}
			else
				{
				bool portrait = Windows[0].Width < Windows[0].Height;
				masterField.Orientation = (portrait ? StackOrientation.Vertical : StackOrientation.Horizontal);
				}
			}

		protected override void OnStart ()
			{
			Current_MainDisplayInfoChanged (null, null);
			base.OnStart ();
			}

		protected override void OnResume ()
			{
			Current_MainDisplayInfoChanged (null, null);
			base.OnResume ();
			}

		/// <summary>
		/// Сохранение настроек программы
		/// </summary>
		protected override void OnSleep ()
			{
			// Сброс текущего решения
			ClearSolution_Clicked (null, null);

			// Сохранение
			FlushMatrix ();
			}

		/// <summary>
		/// Доступные типы уведомлений
		/// </summary>
		public enum TipTypes
			{
			/// <summary>
			/// Первая подсказка
			/// </summary>
			WelcomeTip = 0x02,
			}

		#endregion

		#region О приложении

		// Выбор языка приложения
		private async void SelectLanguage_Clicked (object sender, EventArgs e)
			{
			languageButton.Text = await RDInterface.CallLanguageSelector ();
			}

		// Вызов справочных материалов
		private async void ReferenceButton_Click (object sender, EventArgs e)
			{
			if (RDGenerics.IsTV)
				{
				await RDInterface.ShowMessage (RDLocale.GetText ("HelpQRTip"),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				return;
				}

			await RDInterface.CallHelpMaterials (RDHelpMaterials.ReferenceMaterials);
			}

		private async void HelpButton_Click (object sender, EventArgs e)
			{
			await RDInterface.CallHelpMaterials (RDHelpMaterials.HelpAndSupport);
			}

		// Изменение размера шрифта интерфейса
		private void FontSizeButton_Clicked (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Button b = (Button)sender;
				if (RDInterface.IsNameDefault (b.Text, RDDefaultButtons.Increase))
					RDInterface.MasterFontSize += 0.5;
				else if (RDInterface.IsNameDefault (b.Text, RDDefaultButtons.Decrease))
					RDInterface.MasterFontSize -= 0.5;
				}

			aboutFontSizeField.Text = RDInterface.MasterFontSize.ToString ("F1");
			aboutFontSizeField.FontSize = RDInterface.MasterFontSize;
			}

		#endregion

		#region Рабочая зона

		// Метод открывает страницу О программе
		private async void MenuButton_Clicked (object sender, EventArgs e)
			{
			// Выбор варианта
			if (menuVariants.Count < 1)
				{
				menuVariants.Add ([]);
				menuVariants[0].Add ("🔢\t " + RDLocale.GetText ("Menu0"));
				menuVariants[0].Add ("🕹\t " + RDLocale.GetText ("Menu1"));
				menuVariants[0].Add ("📊\t " + RDLocale.GetText ("StatsButton"));
				menuVariants[0].Add ("⚙️\t " + RDLocale.GetText ("Menu2"));
				menuVariants[0].Add ("ℹ️\t " + RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout));

				menuVariants.Add ([]);
				menuVariants[1].Add ("✅\t " + RDLocale.GetText ("SolveButton"));
				menuVariants[1].Add ("❌\t " + RDLocale.GetText ("ResetField"));
				menuVariants[1].Add ("📄\t " + RDLocale.GetText ("LoadFromFile"));
				menuVariants[1].Add ("💾\t " + RDLocale.GetText ("SaveToFile"));

				menuVariants.Add ([]);
				menuVariants[2].Add ("✨\t " + RDLocale.GetText ("GenerateMatrix"));
				menuVariants[2].Add ("☑️\t " + RDLocale.GetText ("CheckSolutionButton"));
				menuVariants[2].Add ("↩️\t " + RDLocale.GetText ("ClearSolution"));
				}
			List<List<int>> indirectMenu = [
				[0, 1],
				[1, 2],
				];

			// Верхнее меню
			int firstMenu = await RDInterface.ShowList (RDLocale.GetText ("MenuButton") + ":",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), menuVariants[0]);
			if (firstMenu < 0)
				return;

			// Второе меню
			if (indirectMenu[0].Contains (firstMenu))
				{
				firstMenu = indirectMenu[1][indirectMenu[0].IndexOf (firstMenu)];

				int secondMenu = await RDInterface.ShowList (RDLocale.GetText ("MenuButton") + ":",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), menuVariants[firstMenu]);
				if (secondMenu < 0)
					return;

				firstMenu = firstMenu * 10 + secondMenu;
				}

			// Выполнение
			switch (firstMenu)
				{
				// Настройки
				case 3:
					RDInterface.SetCurrentPage (settingsPage, settingsMasterBackColor);
					break;

				// Выполнить решение
				case 10:
					if (!await FindSolution (true))
						await ShowMessage (SudokuArrayMath.FailurePrefix + RDLocale.GetText ("SolutionIsIncorrect"));
					break;

				// Полный сброс
				case 11:
					if (!await RDInterface.ShowMessage (RDLocale.GetText ("ResetWarning"),
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
						return;

					for (int i = 0; i < numberButtons.Count; i++)
						SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.EmptyValue);
					SudokuArrayMath.GameMode = MatrixDifficulty.None;
					break;

				// Загрузка из файла
				case 12:
					await LoadFromFile ();
					break;

				// Сохранение в файл
				case 13:
					await SaveToFile ();
					break;

				// Генерация матрицы
				case 20:
					await GenerateMatrix ();
					break;

				// Проверить корректность решения
				case 21:
					if (!await FindSolution (false))
						await ApplyPenalty ();
					break;

				// Сброс решения
				case 22:
					ClearSolution_Clicked (null, null);
					break;

				// Статистика игры
				case 2:
					ShowScore (false);
					break;

				// О приложении
				case 4:
					RDInterface.SetCurrentPage (aboutPage, aboutMasterBackColor);
					break;
				}
			}

		// Метод применяет штраф
		private static async Task<bool> ApplyPenalty ()
			{
			uint score = SudokuArrayMath.GetScore (ScoreTypes.Penalty);
			SudokuArrayMath.UpdateGameScore (true, score);

			List<string> text = [SudokuArrayMath.FailurePrefix + RDLocale.GetText ("SolutionIsIncorrect")];

			if (SudokuArrayMath.GameMode != MatrixDifficulty.None)
				text.Add ("–" + score.ToString () + SudokuArrayMath.ScoreChar);

			return await ShowMessage (text);
			}

		// Сброс полученного решения
		private void ClearSolution_Clicked (object sender, EventArgs e)
			{
			bool game = (SudokuArrayMath.GameMode != MatrixDifficulty.None);

			for (int i = 0; i < numberButtons.Count; i++)
				{
				if (SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.ContainsFoundValue) ||
					game && SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.ContainsNewValue))
					{
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.EmptyValue);
					}
				}
			}

		// Выбор текущей кнопки в матрице
		private void SelectCurrentButton (object sender, EventArgs e)
			{
			currentButtonIndex = numberButtons.IndexOf ((Button)sender);

			// Кнопка уже была выбрана – выполнить приращение
			bool condition = SudokuArrayMath.CheckCondition (numberButtons[currentButtonIndex], ConditionTypes.SelectedCell);
			if (condition || RDGenerics.IsTV)
				{
				Byte v = 0;
				Button b = numberButtons[currentButtonIndex];

				// В игровом режиме изменение проверенных ячеек запрещено
				if ((SudokuArrayMath.GameMode != MatrixDifficulty.None) &&
					!SudokuArrayMath.CheckCondition (b, ConditionTypes.IsEmpty) &&
					!SudokuArrayMath.CheckCondition (b, ConditionTypes.ContainsNewValue))
					return;

				// Задание значения
				try
					{
					v = SudokuArrayMath.GetDigit (b.Text);
					v++;
					}
				catch { }

				if (v == 0)
					b.Text = SudokuArrayMath.GetAppearance (1);
				else if (v > 9)
					SudokuArrayMath.SetProperty (b, PropertyTypes.EmptyValue);
				else
					b.Text = SudokuArrayMath.GetAppearance (v);

				SudokuArrayMath.SetProperty (b, PropertyTypes.NewColor);
				}

			// Кнопка выбрана впервые – выполнить изменение цветов
			if (!condition || RDGenerics.IsTV)
				PaintButtons ();
			}

		// Переход между кнопками на Android TV
		private void FocusButton (object sender, EventArgs e)
			{
			currentButtonIndex = numberButtons.IndexOf ((Button)sender);
			PaintButtons ();
			}

		// Метод отвечает за обновление цветов кнопок основного поля
		private void PaintButtons ()
			{
			bool showAffected = (SudokuArrayMath.HighlightType != HighlightTypes.None);

			// Обновление цветов
			if (showAffected)
				{
				bool squaresToo = (SudokuArrayMath.HighlightType == HighlightTypes.LinesAndSquares);
				for (int i = 0; i < numberButtons.Count; i++)
					{
					if (i == currentButtonIndex)
						SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.SelectedCell);
					else if (showAffected && SudokuArrayMath.IsCellAffected ((uint)currentButtonIndex,
						(uint)i, squaresToo))
						SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.AffectedCell);
					else
						SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.DeselectedCell);
					}
				}

			// Обновление подсказки
			if (SudokuArrayMath.ShowFreeDigitsFlag)
				{
				if (!SudokuArrayMath.CheckCondition (numberButtons[currentButtonIndex], ConditionTypes.IsEmpty))
					{
					freeDigitsTipButton.Text = "";
					return;
					}

				string existing = "";
				for (int i = 0; i < numberButtons.Count; i++)
					{
					if (SudokuArrayMath.IsCellAffected ((uint)currentButtonIndex, (uint)i, true))
						if (!existing.Contains (numberButtons[i].Text))
							existing += numberButtons[i].Text;
					}

				freeDigitsTipButton.Text = SudokuArrayMath.GetFreeDigitsForCell (existing);
				}
			}

		// Выбор значения для текущей кнопки в матрице
		private void SetValueForCurrentButton (object sender, EventArgs e)
			{
			// Контроль
			if (currentButtonIndex < 0)
				return;

			// Выполнение
			int idx = inputButtons.IndexOf ((Button)sender);
			Button b = numberButtons[currentButtonIndex];

			// В игровом режиме изменение проверенных ячеек запрещено
			if ((SudokuArrayMath.GameMode != MatrixDifficulty.None) &&
				!SudokuArrayMath.CheckCondition (b, ConditionTypes.IsEmpty) &&
				!SudokuArrayMath.CheckCondition (b, ConditionTypes.ContainsNewValue))
				return;

			if (idx > 0)
				b.Text = SudokuArrayMath.GetAppearance ((Byte)idx);
			else
				SudokuArrayMath.SetProperty (b, PropertyTypes.EmptyValue);
			SudokuArrayMath.SetProperty (b, PropertyTypes.NewColor);
			}

		// Метод выполняет решение судоку или его проверку
		private async void SolveSudoku_Clicked (object sender, EventArgs e)
			{
			if (!await FindSolution (true))
				await ShowMessage (SudokuArrayMath.FailurePrefix + RDLocale.GetText ("SolutionIsIncorrect"));

			if (RDGenerics.IsTV && (currentButtonIndex >= 0) && (numberButtons.Count > currentButtonIndex))
				numberButtons[currentButtonIndex].Focus ();
			}

		private async void CheckSolution_Clicked (object sender, EventArgs e)
			{
			if (!await FindSolution (false))
				await ApplyPenalty ();

			if (RDGenerics.IsTV && (currentButtonIndex >= 0) && (numberButtons.Count > currentButtonIndex))
				numberButtons[currentButtonIndex].Focus ();
			}

		private async Task<bool> FindSolution (bool LoadResults)
			{
			// Остановка решения
			if (!numbersField.IsEnabled)
				{
				SudokuArrayMath.RequestStop ();
				return true;
				}

			// Сборка массива
			Byte[,] matrix = new Byte[SudokuArrayMath.SideSize, SudokuArrayMath.SideSize];
			for (int r = 0; r < SudokuArrayMath.SideSize; r++)
				{
				for (int c = 0; c < SudokuArrayMath.SideSize; c++)
					{
					Button ct = numberButtons[r * (int)SudokuArrayMath.SideSize + c];
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
						for (int i = 0; i < numberButtons.Count; i++)
							SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.ErrorColor);

					return false;
				}

			// Решение задачи
			SetInterfaceState (false);

			await Task.Run<bool> (SudokuArrayMath.FindSolution);

			SetInterfaceState (true);

			// Разбор решения
			switch (SudokuArrayMath.CurrentStatus)
				{
				case SolutionResults.NoSolutionsFound:
				case SolutionResults.NotInited:
					if (LoadResults)
						for (int i = 0; i < numberButtons.Count; i++)
							SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.ErrorColor);

					return false;

				case SolutionResults.SearchAborted:	// Не перекрашивать поле
					return true;	// Не считать нарушением правил
				}

			// Игровой режим
			if (!LoadResults)
				{
				// Цвет новых ячеек меняется на фоновый
				uint newCellsCount = 0, emptyCellsCount = 0;
				bool gameMode = (SudokuArrayMath.GameMode != MatrixDifficulty.None);

				for (int i = 0; i < numberButtons.Count; i++)
					{
					if (gameMode)
						{
						if (SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.IsEmpty))
							emptyCellsCount++;
						else if (SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.ContainsNewValue))
							newCellsCount++;
						}

					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.OldColor);
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
						achiLine += " " + achiText.Substring (0, left);

						uint tip = 1u << (int)i;
						if ((RDGenerics.TipsState & tip) == 0)
							{
							await RDInterface.ShowMessage (achiText, RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
							RDGenerics.TipsState |= tip;
							}
						}

					// Обновление счёта
					SudokuArrayMath.UpdateGameScore (false, score);

					// Отображение результата и отключение игрового режима до следующей генерации
					List<string> msgText = [];
					if (win && !SudokuArrayMath.ShowStatsOnWinFlag)
						msgText.Add (RDLocale.GetText ("SolvedText"));
					else if (!win)
						msgText.Add (SudokuArrayMath.SuccessPrefix + RDLocale.GetText ("SolutionIsCorrect"));
					// При выигрыше и переходе на экран статистики эти сообщения пропускаются полностью

					string s = "+" + score.ToString () + SudokuArrayMath.ScoreChar;
					if (!string.IsNullOrWhiteSpace (achiLine))
						s += "\t\t+" + achiLine;
					msgText.Add (s);

					await ShowMessage (msgText);

					// Отобразить решение в случае выигрыша (без return; режим игры отключается далее)
					if (win)
						{
						if (SudokuArrayMath.ShowStatsOnWinFlag)
							ShowScore (true);
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
					await ShowMessage (SudokuArrayMath.SuccessPrefix + RDLocale.GetText ("SolutionIsCorrect"));
					return true;
					}
				}

			// Отображение решения
			SudokuArrayMath.GameMode = MatrixDifficulty.None;
			for (int r = 0; r < SudokuArrayMath.SideSize; r++)
				{
				for (int c = 0; c < SudokuArrayMath.SideSize; c++)
					{
					Button ct = numberButtons[r * (int)SudokuArrayMath.SideSize + c];
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

		// Метод отображает игровую статистику
		private void ShowScore (bool AsWin)
			{
			resultsPage.Title = AsWin ? RDLocale.GetText ("SolvedText") : RDLocale.GetText ("StatsText");

			// Результаты
			string[] stats = SudokuArrayMath.GetStatsValues ();

			scoreField.Text = stats[0];
			achievementField.Text = stats[1];

			// Запуск
			RDInterface.SetCurrentPage (resultsPage, resultsMasterBackColor);
			}

		private async void SendResultToClipboard (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (RDGenerics.DefaultAssemblyVisibleName + RDLocale.RNRN +
				resultsPage.Title + RDLocale.RNRN + ((Button)sender).Text, true);
			}

		// Метод выполняет блокировку / разблокировку интерфейса
		private void SetInterfaceState (bool Enabled)
			{
			numbersField.IsEnabled = menuButton.IsVisible = Enabled;
			solutionTipLabel.IsVisible = !Enabled;
			for (int i = 0; i < inputButtons.Count; i++)
				inputButtons[i].IsEnabled = Enabled;

			if (!Enabled)
				{
				generateButton.IsVisible = clearButton.IsVisible = checkButton.IsVisible = freeDigitsTipButton.IsVisible = false;
				solutionButton.IsVisible = true;
				solutionButton.Text = "❌";
				}
			else
				{
				solutionButton.Text = "✅";
				GameModeSwitch_Toggled (null, null);
				}
			}

		// Метод загружает матрицу из файла
		private async Task<bool> LoadFromFile ()
			{
			// Контроль
			if (!flags.HasFlag (RDAppStartupFlags.CanReadFiles))
				{
				if (await RDInterface.ShowMessage (
					RDLocale.GetDefaultText (RDLDefaultTexts.Message_ReadWritePermission) + "." +
					RDLocale.RNRN + RDLocale.GetDefaultText (RDLDefaultTexts.Message_GoToPermissions),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
					RDInterface.CallAppSettings ();
				return false;
				}

			// Попытка считывания файла
			string file = await RDGenerics.LoadFromFile (RDEncodings.UTF8);
			if (string.IsNullOrWhiteSpace (file))
				return false;

			// Обработка
			string line = SudokuArrayMath.ParseMatrixFromFile (file);
			if (string.IsNullOrWhiteSpace (line))
				{
				await RDInterface.ShowMessage (RDLocale.GetText ("MessageNotEnough"),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				return false;
				}

			// Загрузка
			for (int i = 0; i < numberButtons.Count; i++)
				{
				numberButtons[i].Text = SudokuArrayMath.GetAppearance (line[i].ToString ());
				if (SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.IsEmpty))
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.NewColor);
				else
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.OldColor);
				}

			// Сброс игрового режима
			SudokuArrayMath.GameMode = MatrixDifficulty.None;

			// Успешно
			return true;
			}

		// Метод сохраняет матрицу в файл
		private async Task<bool> SaveToFile ()
			{
			// Контроль
			if (!flags.HasFlag (RDAppStartupFlags.CanWriteFiles))
				{
				if (await RDInterface.ShowMessage (
					RDLocale.GetDefaultText (RDLDefaultTexts.Message_ReadWritePermission) + "." +
					RDLocale.RNRN + RDLocale.GetDefaultText (RDLDefaultTexts.Message_GoToPermissions),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
					RDInterface.CallAppSettings ();
				return false;
				}

			// Выгрузка данных
			FlushMatrix ();
			string file = SudokuArrayMath.BuildMatrixToSave (SudokuArrayMath.SudokuField);

			// Сохранение
			return await RDGenerics.SaveToFile (ProgramDescription.AssemblyMainName + ".txt",
				file, RDEncodings.UTF8);
			}

		// Генерация матрицы судоку
		private async void GenerateMatrix_Clicked (object sender, EventArgs e)
			{
			await GenerateMatrix ();
			}

		private async Task<bool> GenerateMatrix ()
			{
			// Контроль
			if (SudokuArrayMath.GameMode != MatrixDifficulty.None)
				{
				if (!await RDInterface.ShowMessage (RDLocale.GetText ("GameIsNotCompletedMessage"),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
					return false;
				}

			// Выбор сложности
			int res = await RDInterface.ShowList (RDLocale.GetText ("DifficultyLevel"),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), difficultyVariants);
			if (res < 0)
				return false;

			// Запуск
			SetInterfaceState (false);
			solutionButton.IsVisible = false;

			SudokuArrayMath.SetGenerationDifficulty ((MatrixDifficulty)res);
			await Task.Run<bool> (SudokuArrayMath.GenerateMatrix);

			SetInterfaceState (true);

			// Отображение результата
			for (int r = 0; r < SudokuArrayMath.SideSize; r++)
				{
				for (int c = 0; c < SudokuArrayMath.SideSize; c++)
					{
					Button ct = numberButtons[r * (int)SudokuArrayMath.SideSize + c];
					if (SudokuArrayMath.ResultMatrix[r, c] == 0)
						SudokuArrayMath.SetProperty (ct, PropertyTypes.EmptyValue);
					else
						ct.Text = SudokuArrayMath.GetAppearance (SudokuArrayMath.ResultMatrix[r, c]);
					SudokuArrayMath.SetProperty (ct, PropertyTypes.OldColor);
					}
				}

			// Взведение игрового режима
			SudokuArrayMath.GameMode = (MatrixDifficulty)res;

			// Завершено
			return true;
			}

		// Выбор режима приложения
		private void GameModeSwitch_Toggled (object sender, ToggledEventArgs e)
			{
			if (sender != null)
				SudokuArrayMath.AppModeIsGame = gameModeSwitch.IsToggled;

			// Настройка
			bool game = SudokuArrayMath.AppModeIsGame;

			generateButton.IsVisible = checkButton.IsVisible = freeDigitsTipButton.IsVisible = game;
			solutionButton.IsVisible = !game;

			if (!game || !SudokuArrayMath.ShowFreeDigitsFlag)
				freeDigitsTipButton.Text = "";

			clearButton.IsVisible = true;
			if (!game)
				SudokuArrayMath.GameMode = MatrixDifficulty.None;
			}

		// Метод формирует из текущего состояния таблицы сплошную строку и отправляет её на сохранение
		private void FlushMatrix ()
			{
			string sudoku = "";
			for (int i = 0; i < numberButtons.Count; i++)
				sudoku += SudokuArrayMath.GetDigit (numberButtons[i].Text).ToString ();

			SudokuArrayMath.SudokuField = sudoku;
			}

		// Выбор цветовой схемы приложения
		private async void ColorSchemeButton_Clicked (object sender, EventArgs e)
			{
			// Выбор варианта
			if (colorSchemeVariants.Count < 1)
				{
				string[] names = SudokuArrayMath.ColorSchemesNames;
				for (int i = 0; i < names.Length; i++)
					colorSchemeVariants.Add (names[i]);
				}

			int res;
			if (sender != null)
				{
				res = await RDInterface.ShowList (RDLocale.GetText ("ColorSchemeLabel") + ":",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), colorSchemeVariants);
				if (res < 0)
					return;

				SudokuArrayMath.ColorScheme = (uint)res;
				}
			else
				{
				res = (int)SudokuArrayMath.ColorScheme;
				}

			// Настройка
			colorSchemeButton.Text = colorSchemeVariants[res];

			solutionPage.BackgroundColor = SudokuArrayMath.BackgroundColor;
			for (int i = 0; i < numberButtons.Count; i++)
				{
				SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.DeselectedCell);

				// Переназначение цветов для дальнейшей корректной работы метода CheckCondition
				if (SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.ContainsFoundValue))
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.SuccessColor);
				else if (SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.ContainsNewValue))
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.NewColor);
				else if (SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.ContainsErrorValue))
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.ErrorColor);
				else
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.OldColor);
				}

			for (int i = 0; i < inputButtons.Count; i++)
				{
				SudokuArrayMath.SetProperty (inputButtons[i], PropertyTypes.OtherButton);
				SudokuArrayMath.SetProperty (inputButtons[i], PropertyTypes.OldColor);
				}

			SudokuArrayMath.SetProperty (generateButton, PropertyTypes.OtherButton);
			SudokuArrayMath.SetProperty (clearButton, PropertyTypes.OtherButton);
			SudokuArrayMath.SetProperty (checkButton, PropertyTypes.OtherButton);
			SudokuArrayMath.SetProperty (solutionButton, PropertyTypes.OtherButton);
			SudokuArrayMath.SetProperty (menuButton, PropertyTypes.OtherButton);
			SudokuArrayMath.SetProperty (menuButton, PropertyTypes.OldColor);
			SudokuArrayMath.SetProperty (freeDigitsTipButton, PropertyTypes.OtherButton);
			SudokuArrayMath.SetProperty (freeDigitsTipButton, PropertyTypes.OldColor);
			SudokuArrayMath.SetProperty (colorSchemeButton, PropertyTypes.OtherButton);
			SudokuArrayMath.SetProperty (colorSchemeButton, PropertyTypes.OldColor);
			}

		// Выбор цветовой схемы приложения
		private async void CellsAppearanceButton_Clicked (object sender, EventArgs e)
			{
			// Выбор варианта
			if (appearanceVariants.Count < 1)
				{
				string[] names = SudokuArrayMath.CellsAppearancesNames;
				for (int i = 0; i < names.Length; i++)
					appearanceVariants.Add (names[i]);
				}

			int res;
			if (sender != null)
				{
				res = await RDInterface.ShowList (RDLocale.GetText ("CellsAppearanceLabel") + ":",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), appearanceVariants);
				if (res < 0)
					return;

				// Подготовка к настройке для неначального вызова
				FlushMatrix ();
				SudokuArrayMath.CellsAppearance = (uint)res;
				}
			else
				{
				res = (int)SudokuArrayMath.CellsAppearance;
				}

			// Настройка
			cellsAppearanceButton.Text = appearanceVariants[res];

			string line = SudokuArrayMath.SudokuField;
			for (int i = 0; i < numberButtons.Count; i++)
				{
				numberButtons[i].Text = SudokuArrayMath.GetAppearance (line[i].ToString ());
				numberButtons[i].FontSize = SudokuArrayMath.CellsAppearancesFontSize;
				numberButtons[i].FontAttributes = SudokuArrayMath.CellsAppearancesBoldFont ?
					FontAttributes.Bold : FontAttributes.None;

				if ((sender == null) && !SudokuArrayMath.CheckCondition (numberButtons[i], ConditionTypes.IsEmpty))
					SudokuArrayMath.SetProperty (numberButtons[i], PropertyTypes.OldColor);
				}
			for (int i = 1; i < inputButtons.Count; i++)
				{
				inputButtons[i].Text = SudokuArrayMath.GetAppearance ((Byte)i);
				inputButtons[i].FontSize = SudokuArrayMath.CellsAppearancesFontSize;
				}
			}

		// Включение / выключение фиксации экрана
		private void KeepScreenOnSwitch_Toggled (object sender, ToggledEventArgs e)
			{
			RDInterface.KeepScreenOn = keepScreenOnSwitch.IsToggled;
			}

		// Включение / выключение отображения цифр, доступных для выбранной ячейки
		private void ShowFreeDigits_Toggled (object sender, ToggledEventArgs e)
			{
			SudokuArrayMath.ShowFreeDigitsFlag = showFreeDigitsSwitch.IsToggled;
			}

		// Метод отображает сообщения для пользователя в виде нескольких последовательных
		// всплывающих оповещений
		private static async Task<bool> ShowMessage (List<string> Text)
			{
			for (int i = 0; i < Text.Count; i++)
				RDInterface.ShowBalloon (Text[i], false);

			return true;
			}

		private static async Task<bool> ShowMessage (string Text)
			{
			RDInterface.ShowBalloon (Text, true);
			return true;
			}

		// Включение / выключение отображения статистики игры при выигрыше
		private void ShowStatsOnWinSwitch_Toggled (object sender, ToggledEventArgs e)
			{
			SudokuArrayMath.ShowStatsOnWinFlag = showStatsOnWinSwitch.IsToggled;
			}

		// Включение / выключение подсветки простреливаемых ячеек
		private async void HighlightAffectedButton_Clicked (object sender, EventArgs e)
			{
			// Выбор варианта
			if (highlightVariants.Count < 1)
				{
				for (int i = 0; i < 3; i++)
					highlightVariants.Add (RDLocale.GetText ("Highlight" + i.ToString ()));
				}

			int res;
			if (sender != null)
				{
				res = await RDInterface.ShowList (RDLocale.GetText ("HighlightAffectedLabel") + ":",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), highlightVariants);
				if (res < 0)
					return;
				SudokuArrayMath.HighlightType = (HighlightTypes)res;
				}
			else
				{
				res = (int)SudokuArrayMath.HighlightType;
				}

			// Настройка и выполнение
			highlightButton.Text = highlightVariants[res];

			// При запуске приложения этот вызов выполняется далее по сценарию загрузки страницы,
			// поэтому не требует повторения
			if (sender != null)
				ColorSchemeButton_Clicked (null, null);
			}

		// Отображение подсказки к доступным цифрам
		private void FreeDigitsTip_Click (object sender, EventArgs e)
			{
			if (string.IsNullOrWhiteSpace (freeDigitsTipButton.Text) ||
				string.IsNullOrWhiteSpace (SudokuArrayMath.LastFreeDigitsDescription))
				return;

			RDInterface.ShowBalloon (SudokuArrayMath.LastFreeDigitsDescription, true);
			}

		// Загрузка результатов игры
		private async void LoadProfile_Clicked (object sender, EventArgs e)
			{
			// Контроль
			if (!flags.HasFlag (RDAppStartupFlags.CanReadFiles))
				{
				if (await RDInterface.ShowMessage (
					RDLocale.GetDefaultText (RDLDefaultTexts.Message_ReadWritePermission) + "." +
					RDLocale.RNRN + RDLocale.GetDefaultText (RDLDefaultTexts.Message_GoToPermissions),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
					RDInterface.CallAppSettings ();
				return;
				}

			// Попытка считывания файла
			string line = await RDGenerics.LoadFromFile (RDEncodings.UTF8);
			if (SudokuArrayMath.ParseExchangeFile (line))
				ShowScore (false);
			else
				RDInterface.ShowBalloon (RDLocale.GetText ("ScoresExchangeError"), true);
			}

		// Сохранение результатов игры
		private async void SaveProfile_Clicked (object sender, EventArgs e)
			{
			// Контроль
			if (!flags.HasFlag (RDAppStartupFlags.CanWriteFiles))
				{
				if (await RDInterface.ShowMessage (
					RDLocale.GetDefaultText (RDLDefaultTexts.Message_ReadWritePermission) + "." +
					RDLocale.RNRN + RDLocale.GetDefaultText (RDLDefaultTexts.Message_GoToPermissions),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
					RDInterface.CallAppSettings ();
				return;
				}

			// Попытка записи
			string line = SudokuArrayMath.BuildExchangeFile ();
			byte[] file = RDGenerics.GetEncoding (RDEncodings.UTF8).GetBytes (line);
			await RDGenerics.SaveToFile (ProgramDescription.AssemblyMainName +
				SudokuArrayMath.ExchangeFileExt, file);
			}

		#endregion
		}
	}
