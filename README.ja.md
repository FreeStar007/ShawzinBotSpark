[中文](README.md) | [English](README.en.md) | [日本語](README.ja.md)

![**ShawzinBot** ロゴ](https://github.com/FreeStar007/ShawzinBotSpark/blob/main/ShawzinBot/Resources/Shawzin.png)

**ShawzinBot** は、**MIDI** 入力やファイルを一連のキー操作に変換して**ショウジン（Shawzin）**を演奏する小さなツールです。あらゆる **MIDI** 入力（**MIDI** キーボード、仮想 **MIDI** チャンネルなど）に対応しており、標準的な **MIDI** ファイルにも対応しています。

# ダウンロード
[最新版の **ShawzinBot** ダウンロードページ](https://github.com/FreeStar007/ShawzinBotSpark/releases/latest)

# 概要

### ShawzinBot とは？
**ShawzinBot** はサードパーティ製ソフトウェアで、**MIDI** ファイル（.mid）を読み込み、『Warframe（星际战甲）』内でその内容を演奏します。仕組みはキー入力を模擬して**ショウジン**を演奏するものです。

### 安全ですか？
**ShawzinBot** はゲームの実行に一切干渉しません。新しいコードの注入も、既存コードの変更も、メモリへのアクセスも行わないため、チートとは到底言えません。安心してご利用ください。

### どのように実装されていますか？
**ShawzinBot** は **C#** の [DryWetMIDI **MIDI** ライブラリ（作者：melanchall）](https://www.midi.org/specifications/category/smf-specifications) を基に開発されています。このライブラリが **MIDI** ファイルの読み込みと再生を担当し、各ノートイベントはすべて **ShawzinBot** によってインターセプトされ、ノート ID に応じて異なるキーの組み合わせを押して音を鳴らします。さらに外部 **MIDI** デバイス（キーボード、シンセサイザーなど）にも対応しています。

# クイックスタート
使い方は簡単で、コンポーネントをバラバラにしない限り、PC のどこに置いても構いません。インストールは不要で、**ShawzinBot.exe** をダブルクリックするだけで起動します（**ShawzinBot.exe.config** ファイルを削除しないようご注意ください）。

## 使い方
プログラムを起動すると、次のようなウィンドウが表示されます。

![**ShawzinBot** 概要](https://github.com/FreeStar007/ShawzinBotSpark/blob/main/ShawzinBot/Resources/app.png)

左上にボタンがあり、クリックすると **MIDI** ファイルを開いて読み込めます。ファイルを開いたら再生をクリックし、ゲームに戻れば OK です。**ShawzinBot** がキーボードを操作するのは『Warframe』に戻ったときだけで、それ以外で勝手にキーを押すことはありません。

外部 **MIDI** デバイスを使いたい場合は、ドロップダウンメニューから選択すれば演奏できます。デバイスが表示されない場合は、ドロップダウンの右にある更新ボタンをクリックして再読み込みしてください。

プログラムは既定で利用可能なすべての **MIDI** トラックにチェックを入れます。他のトラックに切り替えたり複数選択したりしたい場合は、**MIDI トラック（トラック）** でチェックを付けるだけです。とても簡単です。

現在調整できる設定は **2 つ** あります：
* **ヴィブラート（Vibrato）** - ゲーム内演奏時にビブラートのような効果。既定ではオフ
* **移調（Transpose）** - 演奏できないノートを自動的に移調。既定ではオン

# 特別な感謝
* [@lilggamegenius](https://github.com/lilggamegenius) - ゲーム内のキー割り当ての解決にご協力いただきました
* [/u/T2k5](https://www.reddit.com/user/T2k5/) - 最初の完全なキーボードスクリプトを作成されました

# 注意事項
* 複数のキーを同時に押すと問題が発生します。これは**ショウジン**自体の制限で、ノートをわずらにずらせば解決できます。

# 最新情報
これは元プロジェクトの MIT ライセンスに基づく私が始めた新しいプロジェクトで、元プロジェクトのライセンスを引き継いでいます。
元プロジェクト：https://github.com/ianespana/ShawzinBot
[@ianespana](https://github.com/ianespana) の元プロジェクトのサポートに感謝します。