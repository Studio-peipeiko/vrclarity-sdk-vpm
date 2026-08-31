# VRClarity SDK VPM Repository

VRChatワールドの利用状況に関する匿名の集計値をVRClarityに送信するSDKを配布するVPMレジストリリポジトリです。

詳細は [VRClarity](https://vrclarity.net/) をご覧ください。

## 📦 パッケージ

### VRClarity SDK

VRChatワールド用SDK。滞在時間・移動距離・訪問回数・プラットフォーム・インスタンス内プレイヤー数などの匿名の集計値をVRClarityに送信します。個人を特定する情報は取得・送信しません。

- **パッケージ名**: `net.vrclarity.sdk`
- **バージョン**: 0.7.0
- **公式サイト**: https://vrclarity.net/
- **ドキュメント**: [README (日本語)](./Packages/net.vrclarity.sdk/Documentation~/README_ja.md) | [README (English)](./Packages/net.vrclarity.sdk/Documentation~/README_en.md)

## 🚀 ユーザー向け: パッケージの使い方

### VCC へのレジストリ追加

1. VRChat Creator Companion (VCC) を開く
2. `Settings` → `Packages` → `Add Repository` を選択
3. 以下の URL を入力：
   ```
   https://studio-peipeiko.github.io/vrclarity-sdk-vpm/index.json
   ```
4. `Add` をクリック

### パッケージのインストール

1. VCC でプロジェクトを開く
2. `Manage Project` をクリック
3. パッケージリストから `VRClarity SDK` を探す
4. `+` ボタンをクリックしてインストール

詳しい使い方は [SDK 導入ガイド（オンライン）](https://vrclarity.net/docs/sdk-guide) を参照してください。

## ⚖️ データ利用規約

VRClarity SDK を使用することで、SDK 利用規約に同意したものとみなされます。要点として、送信されたメトリクスデータを集計・統計化したデータセットに関する権利は VRClarity に帰属し、エンドポイントの改変・データの横流し（第三者提供/販売）・VRClarity インフラの不正利用などは禁止されています。

- **SDK 利用規約（全文・データの取り扱い・禁止事項等）**: [ToS.md（日本語・正本）](./ToS.md) | [ToS_en.md (English / reference)](./ToS_en.md)
- **サービス利用規約 / プライバシーポリシー**: https://vrclarity.net/docs/terms ／ https://vrclarity.net/docs/privacy

## 📝 ライセンス

MIT License - 詳細は [LICENSE](./LICENSE) ファイルを参照してください。

## 🆘 サポート

- **お問い合わせフォーム**: [https://vrclarity.net/contact](https://vrclarity.net/contact)
- **メール**: hello@vrclarity.net

## 📚 関連リンク

- [VRClarity](https://vrclarity.net/)
- [VRClarity ドキュメント](https://vrclarity.net/docs)
- [VRChat Creator Companion](https://vcc.docs.vrchat.com/)
- [VPM Package Specification](https://vcc.docs.vrchat.com/vpm/packages/)
- [UdonSharp Documentation](https://udonsharp.docs.vrchat.com/)

## 📌 変更履歴

各バージョンの変更内容は [GitHub Releases](https://github.com/studio-peipeiko/vrclarity-sdk-vpm/releases) を参照してください。

---

Made with ❤️ by studio peipeiko
