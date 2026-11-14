# Neon Swarm

Mobilna gra zręcznościowa 2D (top-down arena shooter) na Androida, zrealizowana w silniku **Unity 2022.3 LTS** w C#.
Gracz steruje neonowym statkiem na zamkniętej arenie i odpiera kolejne fale roju dronów. Celem jest przetrwanie wszystkich fal na trzech coraz trudniejszych poziomach.

## Opis gry

| | |
|---|---|
| Gatunek | arcade / top-down shooter 2D |
| Platforma | Android 7.0+ (API 24+), orientacja pionowa |
| Cel gry | pokonać wszystkie fale na poziomach 1–3; ukończenie poziomu 3 = zwycięstwo |
| Przegrana | utrata wszystkich punktów życia (HP) |
| Poziomy | 3 (łatwy / średni / trudny), kolejny odblokowuje się po ukończeniu poprzedniego |
| Przeciwnicy | Dryfer (ruch poziomy), Łowca (śledzi gracza i szarżuje), Strzelec (atakuje z dystansu) |
| Przedmioty | moneta (+10 pkt), apteczka (+1 HP), tarcza (5 s nieśmiertelności), turbo (6 s 2× szybkostrzelność) |

### Punktacja

| Akcja | Punkty |
|---|---|
| Pokonanie Dryfera / Łowcy / Strzelca | +100 / +150 / +200 |
| Zebranie monety | +10 |
| Ukończenie poziomu | +500 |
| Bonus czasowy | +50 za każde rozpoczęte 10 s poniżej czasu „par” |
| Poziom bez obrażeń | +250 |

### Funkcje rozszerzone

- ranking 10 najlepszych wyników (lokalnie, z nazwą gracza i datą),
- system osiągnięć (7) i misji (3 aktywne naraz, z nagrodami w monetach),
- waluta (monety) i sklep: skórki statku, broń (podwójne działo), ulepszenia, power-up „tarcza startowa”,
- dynamiczna trudność (wskaźnik TEMPO na HUD),
- proceduralne generowanie przeszkód areny i składu fal (losowanie ważone z ziarnem),
- object pooling pocisków, przeciwników, przedmiotów i efektów,
- zapis do pliku JSON (`save.json` w `Application.persistentDataPath`) z kopią zapasową.

## Sterowanie

| Gest | Akcja |
|---|---|
| Dotknięcie i przeciągnięcie w lewej dolnej części ekranu | pływający wirtualny joystick, czyli ruch statku |
| (automatycznie) | statek celuje i strzela w najbliższego wroga |
| Przycisk **PULS** (prawy dolny róg) | fala uderzeniowa: obrażenia i odrzut wrogów w pobliżu, kasuje wrogie pociski |
| Przycisk pauzy (prawy górny róg) lub systemowe **Wstecz** | pauza |

W edytorze Unity można też grać klawiaturą (WASD / strzałki) i myszą.

## Wymagania

- Unity **2022.3.62f3** (LTS) z modułem **Android Build Support** (OpenJDK, Android SDK & NDK Tools),
- Windows 10/11 lub macOS,
- do testów: urządzenie z Androidem 7.0+ albo emulator Android (AVD x86_64, API 34+).

## Uruchomienie projektu

1. Otwórz folder `NeonSwarm` w Unity Hub (*Add project from disk*) i uruchom go w wersji 2022.3.62f3.
2. Otwórz scenę `Assets/Scenes/Boot.unity` i naciśnij **Play**.
3. Grafika, dźwięki, animacje, prefaby i sceny są generowane kodem. Menu **Neon Swarm → Rebuild Generated Content** odtwarza je od zera.

Budowanie APK z edytora: **Neon Swarm → Build Android APK** (wynik: `Builds/Android/NeonSwarm.apk`).
Budowanie z linii poleceń:

```bash
Unity.exe -batchmode -projectPath <ścieżka>/NeonSwarm -buildTarget Android -executeMethod NeonSwarm.EditorTools.AndroidBuilder.BuildApkBatch -logFile build.log
```

Testy: 12 testów EditMode (logika: zapis, ranking, sklep, misje, punktacja, fale, trudność) i 8 testów PlayMode (ładowanie scen, start poziomu, pauza, śmierć i Game Over, ukończenie poziomu, testy regresyjne). W edytorze: *Window → General → Test Runner → Run All*. Z linii poleceń:

```bash
Unity.exe -batchmode -projectPath <ścieżka>/NeonSwarm -runTests -testPlatform EditMode -testResults results.xml
```

Wszystko naraz (generowanie zawartości, testy EditMode i PlayMode, build APK) uruchamia skrypt PowerShell:

```bash
powershell -ExecutionPolicy Bypass -File Tools/verify.ps1
```

Dokumentację PDF (`Docs/Dokumentacja_NeonSwarm.pdf`) składa z plików `Docs/src/*.html` skrypt `Tools/build-docs.ps1` (wymaga przeglądarki Microsoft Edge).

## Instalacja APK

**Na telefonie:** skopiuj `NeonSwarm.apk` na urządzenie, otwórz plik i zezwól na instalację z nieznanych źródeł.

**Przez ADB (telefon z włączonym debugowaniem USB lub emulator):**

```bash
adb install -r Builds/Android/NeonSwarm.apk
```

**Na emulatorze:** uruchom AVD w Android Studio (Device Manager) i przeciągnij plik APK na okno emulatora.

## Wersja przeglądarkowa (WebGL) – iPhone

Plik APK działa tylko na Androidzie. Na iPhonie (i na komputerze) można grać w wersję przeglądarkową:

- gotowa paczka: `Builds/NeonSwarm-WebGL.zip` (7 MB), zawartość także w `Builds/WebGL/`,
- budowanie: menu **Neon Swarm → Build WebGL** albo `-executeMethod NeonSwarm.EditorTools.WebGLBuilder.BuildWebGLBatch -buildTarget WebGL`.

**Publikacja na itch.io (darmowa, działa na iPhonie):**

1. Załóż konto na itch.io i wybierz *Upload new project*.
2. *Kind of project*: **HTML**. Wgraj `NeonSwarm-WebGL.zip` i zaznacz *This file will be played in the browser*.
3. *Embed options*: rozmiar 540×960, zaznacz *Mobile friendly* (orientacja *Portrait*) i *Fullscreen button*.
4. Otwórz link do gry w Safari na iPhonie. *Udostępnij → Do ekranu początkowego* tworzy ikonę jak aplikacja.

**Test w sieci domowej (bez publikacji):**

```bash
node Tools/serve-webgl.js --lan
```

Następnie na iPhonie podłączonym do tej samej sieci Wi-Fi otwórz w Safari adres wypisany przez skrypt (`http://192.168.x.x:8080`). Przy pierwszym uruchomieniu Windows może zapytać o zezwolenie dla Node.js w zaporze.

Różnice w wersji przeglądarkowej:
- zapis gry trafia do pamięci przeglądarki (IndexedDB przez `PlayerPrefs`),
- głośność ustawiana jest bezpośrednio na źródłach dźwięku, bo Unity nie obsługuje AudioMixera w WebGL,
- nie ma przycisku „Wyjście”,
- dźwięk startuje po pierwszym dotknięciu ekranu, co wynika z zasad przeglądarek.

## Zastosowane technologie

- Unity 2022.3 LTS, C# (.NET Standard 2.1), skrypty podzielone na assembly definitions,
- Unity UI (uGUI) z `CanvasScaler` (1080×1920) i obsługą *safe area*,
- Physics 2D: `Rigidbody2D`, `CircleCollider2D`/`BoxCollider2D`, triggery, macierz kolizji warstw,
- Animator (Idle/Run/Attack/Hurt/Dead) + `AnimatorOverrideController`,
- AudioMixer (grupy Master/Music/SFX, parametry eksponowane), pula `AudioSource`,
- ParticleSystem, Sprite Atlas, kompresja ASTC, IL2CPP (ARM64 + x86_64),
- `JsonUtility` (zapis), Unity Test Framework (NUnit).

## Struktura projektu

```
Assets/
├── Scenes/        Boot, MainMenu, Settings, Level1, Level2, Level3, GameOver
├── Scripts/
│   ├── Core/      zdarzenia, sesja gry, pule obiektów, ładowanie scen, bootstrap
│   ├── Player/    ruch, zdrowie, strzelanie, puls
│   ├── Enemy/     EnemyBase + Dryfer, Łowca, Strzelec, rejestr wrogów
│   ├── Combat/    pociski
│   ├── Items/     moneta, apteczka, tarcza, turbo
│   ├── Game/      GameManager, fale, trudność, arena, punkty, kamera, efekty
│   ├── Progress/  sklep, misje, osiągnięcia, ranking
│   ├── UI/        HUD, joystick, menu, panele, pauza, ustawienia
│   ├── Audio/     AudioManager, biblioteka dźwięków
│   └── Save/      SaveData, SaveSystem (JSON)
├── Editor/        generator zawartości i build APK
├── Tests/         testy EditMode
├── Prefabs/  Sprites/  Animations/  Audio/  Materials/  Data/  Resources/
```

## Autorzy

- Patrycja Brodzik (nr albumu 50708)

## Wersja

- Gra: 1.0.0 (versionCode 1)
- Unity: 2022.3.62f3

## Znane problemy

- Na Unity 2022.3 w wersji Personal przy starcie wyświetla się ekran „Made with Unity”, którego nie da się wyłączyć.
- Emulator bez akceleracji GPU (`hw.gpu.enabled=no`) renderuje programowo, więc gra może tam działać poniżej 60 FPS. Na prawdziwym telefonie działa płynnie.
- Ranking jest lokalny (bez serwera). Reinstalacja aplikacji usuwa zapis.
- Nazwa gracza jest ustawiana w Ustawieniach, a nie po zakończeniu gry.
- Unity 2022.3 nie wspiera oficjalnie WebGL na przeglądarkach mobilnych. Wersję przeglądarkową przetestowano w Chromium w trybie telefonu (dotyk, zapis, polskie znaki); na prawdziwym iPhonie nie była jeszcze sprawdzona.
