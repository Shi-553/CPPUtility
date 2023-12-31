# CPPUtility
Visual Studio 2022 の拡張機能。

C++を書くときに使える機能の詰め合わせになる予定です。

インストールは[Releases](https://github.com/Shi-553/CPPUtility/releases)からCPPUtility.vsixをダウンロードしてダブルクリック。

&nbsp;

## Create Header Comment
ドキュメントの先頭 ＋ 全てのヘッダー関数にコメントのスニペットを追加します。

その後、`{Edit}`にカーソルが移動して一つずつ編集できます。(何もせず改行するとスキップ)

### オプション
* ドキュメントの先頭コメントスニペットの編集（置き換え機能付き）
* ヘッダー関数にコメントをつけるかどうか

&nbsp;

## Generate CPP Comment
ヘッダーのコメントを元にドキュメントの先頭＋全てのCPP関数のコメントを自動生成します。

### オプション
* CPP関数にコメントをつけるかどうか
* CPP関数コメントスニペットの編集（置き換え機能付き）

&nbsp;

## Format Variables
アクティブなドキュメントの変数と引数を定義した方法でフォーマットします。（現在キャメルケースのみ対応）

UnrealEngineの場合、[これ](https://forums.unrealengine.com/t/for-those-who-suffer-from-visual-studio-intellisense-slowness/49331)を設定したほうがいいかも

### オプション
* フォーマット済みかどうかを判断するためのRegex
* どんなときに、どういうフォーマットをするかをGUIで設定
