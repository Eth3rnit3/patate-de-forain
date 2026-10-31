"""Génère une image d'un fichier de prompts (assets/**/prompts-vNN.json) avec Codex CLI.

    python3 tools/codex-image/generate.py assets/stages/austerlitz prompts-v01.json 0

Codex tourne dans le dossier de l'asset (il ne peut écrire que là), avec les
images de référence de l'entrée en -i, dans l'ordre, chemins depuis la racine
du dépôt. Il enregistre l'image au chemin « source » de l'entrée. Modèle :
variable CODEX_MODEL, sinon gpt-5.6-sol (celui qu'accepte un compte ChatGPT).
"""
import json
import os
import subprocess
import sys

root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
folder, prompts, index = os.path.abspath(sys.argv[1]), sys.argv[2], int(sys.argv[3])
entry = json.load(open(os.path.join(folder, prompts), encoding="utf-8"))[index]
message = entry["prompt"] + "\n\n" + entry["codexInstructions"].replace("<source>", entry["source"])
command = ["codex", "exec", "--skip-git-repo-check", "-m", os.environ.get("CODEX_MODEL", "gpt-5.6-sol"),
           "--enable", "image_generation", "-s", "workspace-write", "-C", folder]
for image in entry["images"]:
    command += ["-i", os.path.join(root, image)]
command += ["--", message]
sys.exit(subprocess.run(command, stdin=subprocess.DEVNULL).returncode)
