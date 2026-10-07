import importlib.util
import json
from pathlib import Path
import shutil
import sys
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))
import version


class VersionTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = Path(self.tmp.name)
        for path in ["src/AxtralProjection/AxtralProjection.csproj", "src/AxtralProjection/Plugin.cs", "package/manifest.json", "package/icon.png", "CHANGELOG.md"]:
            target = self.root / path
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(ROOT / path, target)

    def tearDown(self):
        self.tmp.cleanup()

    def test_matching_versions_pass(self):
        self.assertEqual(version.validate(self.root), "0.2.0")

    def test_each_version_mismatch_fails(self):
        for name in ["src/AxtralProjection/Plugin.cs", "package/manifest.json", "CHANGELOG.md"]:
            with self.subTest(name=name):
                path = self.root / name
                original = path.read_text()
                path.write_text(original.replace("0.2.0", "0.9.0"))
                with self.assertRaises(ValueError):
                    version.validate(self.root)
                path.write_text(original)

    def test_invalid_manifest_name_fails(self):
        path = self.root / "package/manifest.json"
        data = json.loads(path.read_text())
        data["name"] = "with spaces"
        path.write_text(json.dumps(data))
        with self.assertRaises(ValueError):
            version.validate(self.root)

    def test_bad_icon_fails(self):
        (self.root / "package/icon.png").write_bytes(b"not a png")
        with self.assertRaises(ValueError):
            version.validate(self.root)

    def test_truncated_png_header_fails_cleanly(self):
        (self.root / "package/icon.png").write_bytes(b"\x89PNG\r\n\x1a\n" + b"\x00\x00\x00\x0dIHDR")
        with self.assertRaises(ValueError):
            version.validate(self.root)

    def test_notes_extract_only_current_section(self):
        text = version.notes(self.root)
        self.assertIn("Exhaustion", text)
        self.assertNotIn("Initial compiled", text)


class ReleaseTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        spec = importlib.util.spec_from_file_location("release", ROOT / "scripts/release.py")
        assert spec is not None and spec.loader is not None
        cls.release = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(cls.release)

    def test_dirty_tree_refused_before_build(self):
        with patch.object(self.release, "git", return_value=" M README.md"), patch.object(self.release, "build") as build:
            with self.assertRaises(ValueError):
                self.release.preflight(ROOT, "0.2.0", dry_run=True)
            build.assert_not_called()

    def test_local_tag_refused(self):
        def git(*args, **kwargs):
            return "refs/tags/v0.2.0" if args[0] == "tag" else ""
        with patch.object(self.release, "git", side_effect=git):
            with self.assertRaises(ValueError):
                self.release.preflight(ROOT, "0.2.0", dry_run=True)

    def test_remote_tag_refused(self):
        def git(*args, **kwargs):
            return "deadbeef\trefs/tags/v0.2.0" if args[0] == "ls-remote" and "--tags" in args else ""
        with patch.object(self.release, "git", side_effect=git):
            with self.assertRaises(ValueError):
                self.release.preflight(ROOT, "0.2.0", dry_run=True)

    def test_publish_requires_main(self):
        def git(*args, **kwargs):
            return "feature" if args[0] == "branch" else ""
        with patch.object(self.release, "git", side_effect=git):
            with self.assertRaises(ValueError):
                self.release.preflight(ROOT, "0.2.0", dry_run=False)

    def test_remote_head_mismatch_refused(self):
        def git(*args, **kwargs):
            if args[0] == "branch": return "main"
            if args[0] == "rev-parse": return "localsha"
            if args[0] == "ls-remote" and "--heads" in args: return "othersha\trefs/heads/main"
            return ""
        with patch.object(self.release, "git", side_effect=git):
            with self.assertRaises(ValueError):
                self.release.preflight(ROOT, "0.2.0", dry_run=False)

    def test_dry_run_builds_but_never_mutates_git_or_publishes(self):
        with patch.object(self.release, "preflight"), patch.object(self.release, "validate", return_value="0.2.0"), patch.object(self.release, "build", return_value=ROOT / "artifacts/AxtralProjection-0.2.0.zip") as build, patch.object(self.release, "run") as run:
            self.release.main(["--dry-run"])
            build.assert_called_once()
            run.assert_not_called()


if __name__ == "__main__":
    unittest.main()
