# -*- coding: utf-8 -*-

import sys
sys.path.append("resolvers")

import gyazo
import hatena_fotolife
import imgly
import pckles
import twitpic

#アルファベット順

services = [
	gyazo.gyazo(),
	hatena_fotolife.hatenaFotolife(),
	imgly.imgly(),
	pckles.pckles(),
	twitpic.twitpic()
]
