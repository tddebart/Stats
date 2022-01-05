# Statistics mod
A mod that adds statistics to the game like how many times you blocked or what was your highest damage dealt.

## Adding extra stats
You can add your own stats to the stat menu by adding a void to the action `extraStatsMenu` in Stats like this:
```c#
public void Start() {
    Stats.extraStatsMenu += MyStats;
}


public void MyStats(GameObject menu) {
{
    var MyStatsMenu = Stats.CreateMenu("MyStats", menu);
    var myCustomStats = Stats.CreateStat("My custom stat", "MyStats", MyStatsMenu);
}
```
and then you can update the stat like this:
```c#
void MyUpdateMethod() {
    // add's 1 or the given amount to the stat
    Stats.AddValue("My custom stat");
    
    // or you could 
    
    // set's the stat to the given amount
    Stats.SetValue("My custom stat", 10);
}
```

## Patch notes
- 1.0.0 (Initial release)
