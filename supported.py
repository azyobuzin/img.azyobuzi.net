# -*- coding: utf-8 -*-

import sys
sys.path.append("resolvers")

import flickr
import gyazo
import hatena_fotolife
import imgly
import lockerz
import pckles
import twitpic
import viame
import yfrog

#アルファベット順

services = [
	flickr.flickr(),
	gyazo.gyazo(),
	hatena_fotolife.hatenaFotolife(),
	imgly.imgly(),
	lockerz.lockerz(),
	pckles.pckles(),
	twitpic.twitpic(),
	viame.viame(),
	yfrog.yfrog()
]
