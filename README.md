![**ShawzinBot**Logo](https://github.com/FreeStar007/ShawzinBotSpark/blob/master/ShawzinBot/Resources/Shawzin.png)

**ShawzinBot**是一个能把**MIDI**输入或文件转换成一系列按键操作、用来弹 Shawzin 的小工具，任何**MIDI**输入都能用（MIDI 键盘、虚拟**MIDI**通道之类的都行），也支持标准**MIDI**文件

# 下载
最新版的ShawzinBot地址[这里](https://github.com/FreeStar007/ShawzinBotSpark/releases/latest)

# 关于

### ShawzinBot是什么
**ShawzinBot**是一款第三方软件，能读取**MIDI**文件（.mid）并在《星际战甲》里把内容弹出来，原理就是模拟按键，Shawzin 就是这么个玩法

### 安全吗
**ShawzinBot**完全不会干扰游戏运行，不注入新代码，也不修改任何已有代码，更不会碰内存，所以压根算不上外挂，放心用就好

### 怎么实现的
**ShawzinBot**基于**C#**的 [DryWetMIDI**MIDI**库（作者 melanchall）](https://www.midi.org/specifications/category/smf-specifications) 开发，这个库负责加载并播放**MIDI**文件，每个音符事件都会被 **ShawzinBot**拦下来，根据音符 ID 按下不同的按键组合来发声，另外它还支持外接**MIDI**设备（键盘、合成器之类的）

# 快速上手
用起来挺简单的，只要别把组件拆散，它放电脑哪儿都行，不需要安装，双击**ShawzinBot.exe**就能跑起来（记住不能删掉**ShazinBot.exe.config**文件）

## 用法
程序跑起来后你会看到这样的窗口

![**ShawzinBot**Overview](https://github.com/FreeStar007/ShawzinBotSpark/blob/main/ShawzinBot/Resources/app.png)

左上角有个按钮，点它就能打开并加载**MIDI**文件，文件打开后点播放、切回游戏就行了，**ShawzinBot**只有在你切回《星际战甲》时才会动你的键盘，平时不会瞎按

想用外接**MIDI**设备的话，从下拉菜单里选一下就能弹了，要是没看到你的设备，点下拉框右边的刷新按钮重新加载一下

程序默认会勾选所有可用的**MIDI**音轨，想换别的音轨或多选，在“MIDI追踪（音轨）”处勾选就行，很简单

目前有2个可以调的设置：
* 哇音 - 游戏里演奏时类似颤音，默认关闭
* 移调 - 自动移调那些弹不了的音符，默认开启

# 特别感谢
* [@lilggamegenius](https://github.com/lilggamegenius) - 帮忙搞定了游戏里的按键映射
* [/u/T2k5](https://www.reddit.com/user/T2k5/) - 写了最初的完整键盘脚本

# 注意事项
* 同时按多个键会有问题，这是三线琴本身的局限，把音符稍微错开一点点就能解决

# 新消息
我基于原项目地址的MIT许可开的新坑，沿用原项目许可
原项目地址：https://github.com/ianespana/ShawzinBot
感谢[@ianespana](https://github.com/ianespana)大佬的原项目支持
