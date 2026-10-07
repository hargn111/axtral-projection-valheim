#!/usr/bin/env python3
from version import validate
try:
    print("Version consistent:", validate())
except (ValueError, OSError) as error:
    raise SystemExit(str(error))
