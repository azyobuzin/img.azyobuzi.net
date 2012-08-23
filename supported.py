# -*- coding: utf-8 -*-

import sys
sys.path.append("resolvers")

import hatena_fotolife
import twitpic

#アルファベット順

services = [
	hatena_fotolife.hatenaFotolife(),
	twitpic.twitpic()
]
