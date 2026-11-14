using UnityEngine;

namespace NeonSwarm
{
    public class InstructionsPanel : MenuPanel
    {
        private const string Text =
            "<b><color=#27E8FF>CEL GRY</color></b>\n" +
            "Rój neonowych dronów atakuje arenę. Przetrwaj wszystkie fale na każdym z 3 poziomów. " +
            "Pokonanie ostatniej fali kończy poziom, a ukończenie poziomu 3 wygrywa grę. Utrata wszystkich HP oznacza koniec gry.\n\n" +
            "<b><color=#27E8FF>STEROWANIE</color></b>\n" +
            "• Przyłóż kciuk w lewej dolnej części ekranu – pojawi się wirtualny joystick, którym poruszasz statkiem.\n" +
            "• Statek strzela automatycznie do najbliższego wroga.\n" +
            "• Przycisk PULS (prawy dół) wyzwala falę uderzeniową, która rani wrogów w pobliżu i niszczy ich pociski.\n" +
            "• Przycisk pauzy (prawy górny róg) lub systemowy przycisk Wstecz zatrzymuje grę.\n\n" +
            "<b><color=#FF3DCB>PRZECIWNICY</color></b>\n" +
            "• Dryfer (różowy romb) – porusza się poziomo i odbija od ścian. 1 HP, +100 pkt.\n" +
            "• Łowca (pomarańczowa gwiazda) – śledzi gracza i wykonuje szarżę. 2 HP, +150 pkt.\n" +
            "• Strzelec (zielony sześciokąt) – trzyma dystans i strzela pociskami. 3 HP, +200 pkt.\n\n" +
            "<b><color=#FFD23F>PRZEDMIOTY</color></b>\n" +
            "• Moneta: +10 pkt i +1 moneta do sklepu.\n" +
            "• Apteczka: +1 HP (przy pełnym zdrowiu +25 pkt).\n" +
            "• Tarcza: 5 s nieśmiertelności.\n" +
            "• Turbo: 6 s podwójnej szybkostrzelności.\n\n" +
            "<b><color=#4BFF8A>PUNKTACJA</color></b>\n" +
            "Pokonanie wroga 100–200 pkt · Moneta 10 pkt · Ukończenie poziomu 500 pkt · " +
            "Bonus czasowy 50 pkt za każde rozpoczęte 10 s poniżej czasu par · Poziom bez obrażeń 250 pkt.\n\n" +
            "Gra sama dopasowuje tempo (TEMPO na HUD): gdy idzie Ci dobrze – wrogów jest więcej i są szybsi, gdy tracisz HP – tempo spada.";

        protected override string Title => "INSTRUKCJA";

        protected override void BuildBody(RectTransform container)
        {
            var scroll = UiKit.ScrollList("Scroll", container, out var content);
            UiKit.Stretch(scroll);
            var label = UiKit.Label("Text", content, Text, 40, UiTheme.Current.text, TextAnchor.UpperLeft);
            label.lineSpacing = 1.1f;
        }
    }
}
