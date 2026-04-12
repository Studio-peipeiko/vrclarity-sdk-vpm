# VRClarity SDK VPM Repository

VRChatワールドのプレイヤー行動を計測しVRClarityに送信するSDKを配布するVPMレジストリリポジトリです。

<!-- TODO: サービス開始後に有効化する -->
<!-- 詳細は [VRClarity](https://vrclarity.net/) をご覧ください。 -->

## 📦 パッケージ

### VRClarity SDK

VRChatワールド用SDK。滞在時間・移動距離・訪問回数・プラットフォーム・インスタンス内プレイヤー数を計測し、VRClarityに送信します。

- **パッケージ名**: `net.vrclarity.sdk`
- **バージョン**: 0.1.0
<!-- - **公式サイト**: https://vrclarity.net/ -->
- **ドキュメント**: [README (日本語)](./Packages/net.vrclarity.sdk/Documentation~/README_ja.md) | [README (English)](./Packages/net.vrclarity.sdk/Documentation~/README_en.md)

## 🚀 ユーザー向け: パッケージの使い方

### VCC へのレジストリ追加

1. VRChat Creator Companion (VCC) を開く
2. `Settings` → `Packages` → `Add Repository` を選択
3. 以下の URL を入力：
   ```
   https://Studio-peipeiko.github.io/vrclarity-sdk-vpm/index.json
   ```
4. `Add` をクリック

### パッケージのインストール

1. VCC でプロジェクトを開く
2. `Manage Project` をクリック
3. パッケージリストから `VRClarity SDK` を探す
4. `+` ボタンをクリックしてインストール

詳しい使い方は [パッケージの README (日本語)](./Packages/net.vrclarity.sdk/Documentation~/README_ja.md) を参照してください。

## ⚖️ データ利用規約

VRClarity SDK を使用することで、以下の条件に同意したものとみなされます：

### データの所有権
- **VRClarityに送信されたメトリクスデータの所有権は VRClarity に帰属します**
- ワールド制作者は、データ送信に同意することでSDKを利用できますが、送信されたデータ自体の所有権は保持しません
- VRClarity は収集したデータを匿名化・集計し、サービス改善や統計情報の提供に使用する権利を有します

### 禁止事項
以下の行為は**厳重に禁止**されています：

1. **エンドポイントの変更・改変**
   - SDK が送信するデータのエンドポイント URL を変更すること
   - VRClarity のサーバー以外にデータを送信するように改変すること

2. **データの横流し・不正利用**
   - 送信されたデータを第三者に提供・販売すること
   - SDK を改変して独自のデータ送信システムとして利用すること
   - VRClarity のインフラを不正に利用すること

3. **SDK の悪用**
   - SDK を逆コンパイル・リバースエンジニアリングして独自のシステムに転用すること
   - 暗号化キーやエンドポイント情報を抽出・公開すること

違反が確認された場合、SDK の利用停止やワールドデータの削除、法的措置を取る場合があります。

詳細は [利用規約 (ToS)](./ToS.md) を参照してください。

## 📝 ライセンス

MIT License - 詳細は [LICENSE](./LICENSE) ファイルを参照してください。

## 🆘 サポート

- **X (Twitter)**: [@peipeiko666](https://x.com/peipeiko666)

## 📚 関連リンク

<!-- - [VRClarity](https://vrclarity.net/) -->
- [VRChat Creator Companion](https://vcc.docs.vrchat.com/)
- [VPM Package Specification](https://vcc.docs.vrchat.com/vpm/packages/)
- [UdonSharp Documentation](https://udonsharp.docs.vrchat.com/)

## 📌 バージョン履歴

- **v0.2.0**: SDK セットアップの改善
  - World ID フィールドを削除し、VRC_SceneDescriptor の Blueprint ID から自動取得
  - Bake URLs ボタンを削除し、ビルド時・Playモード開始時に自動ベイク
  - Blueprint ID 未設定またはデタッチ時はURLプールをクリアしリクエスト送信を防止
  - エディタ Playモード時のみURLリクエストのデバッグログを出力
  - URL Pool Status の合計数表記を修正（119 → 121）
  - プレハブの `_platformUrls` 欠落を修正
  - ドキュメント・ToS の表現を「収集」→「送信」に統一
  - ToS 3.3 を MIT ライセンスと整合する内容に修正

- **v0.1.0**: 初回リリース
  - 滞在時間マイルストーン（9段階: 1分〜480分）
  - 移動距離マイルストーン（6段階: 10m〜2500m）
  - 訪問回数のバケット方式追跡（20バケット、最大200回）
  - プラットフォーム検出（PCVR / Desktop / Quest / Android / iOS）
  - プレイヤー数追跡（0〜80人、5分ごと）
  - AES-256-GCM暗号化によるURL事前ベイク

詳細はリリースノートを参照してください。

---

Made with ❤️ by Studio peipeiko
