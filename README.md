# Abgabe 2 Doom 
Kristian Kessel 218933

Unity Version: 6000.3.11f1

### Die Zip-Datei mit lauffähigem Projekt und die Video-Demo liegen im Root des Repos!

## Verwendete Assets

- Asset 1: Swat Character inkl. Idle animation (Mixamo.com)
- Asset 2,3,4,5,6,7 & 8: Firing Rifle, Hit Reaction, Idle to Running, Rifle Run, Rifle Run to Dying, Rifle Run to Stop & Dying Animationen (ebenfalls Mixamo)

## Steuerung

| Taste | Funktion |
| **W** | Vorwärts bewegen |
| **S** | Rückwärts bewegen |
| **A** | Nach links bewegen |
| **D** | Nach rechts bewegen |
| **Maus** | Umschauen / Blickrichtung ändern |
| **LMB (Linke Maustaste)** | Schießen |
| **Q** | Bombe schießen |
| **E** | An eine Kletterwand hängen |
| **CapsLock** | Dash ausführen |
| **RMB** | Grappling Hook benutzen |

## Beschreibung des Projektes
Dieses Projekt dient als Abgabe für Doom vs Fortnite mit A: High Mobility und A: Animierte Bots Aufgabenteilen.
Die Map habe ich so gebaut, dass der Spieler zuerst die Grundlagen des Spiels alle einmal verwenden muss und sie kennen lernt, gefolgt von einer Mansion mit einigen Gegnern, welche deswegen nur von oben(mithilfe des Grappling Hooks und wall-climbing mechanik erreichbar) von Gegnern bereinigt werden kann.
Sowohl der Spieler als auch die Bots haben eine UI-Healthbar, beim ablaufen der HP des Spielers wird dann ein Deathscreen angezeigt.
Der Spieler Charakter kann Sprinten, Doppel-Jumpen, an bestimmten Blöcken den grappling hook verwenden, in der Luft steuern und springen sowie mit einem kurzen Cooldown Dashen, was direkte A- und Decceleration bedeutet.

Die Bots haben eine Detection Range, außerhalb derer sie den Spieler nicht verfolgen, eine Angriffsrange innerhalb derer sie den Spieler im Stehen angreift wenn Sichtkontakt besteht und schießt mit einem cooldown auf diesen. 

## Besondere Herausforderungen / Lessions Learned
Die Übergänge zwischen den Animationen der Bots waren nicht sauber, der Charakter wird am Ende der Run-Animation bspw. immer zurück-gerubberbanded, um wieder in einer Position mit dem Agent zu sein, der die Bewegung ja steuert. Ich konnte auch mit gestaffelten, kürzeren oder vorgezogenen Übergängen nicht komplett dagegen arbeiten, weshalb diese immer noch etwas buggy sind.
