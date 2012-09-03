# -*- coding: utf-8 -*-

import sys
sys.path.append("resolvers")

import flickr
import fxcamera
import gyazo
import hatena_fotolife
import imgly
import imgur
import instagram
import lockerz
import mobypicture
import movapic
import niconico_seiga
import owly
import pckles
import photozou
import pixiv
import tinami
import twipple
import twitgoo
import twitpic
import twitvideo
import viame
import yfrog

#アルファベット順

services = [
	flickr.flickr(),
	fxcamera.fxcamera(),
	gyazo.gyazo(),
	hatena_fotolife.hatenaFotolife(),
	instagram.instagram(),
	imgly.imgly(),
	imgur.imgur(),
	lockerz.lockerz(),
	mobypicture.mobypicture(),
	movapic.movapic(),
	niconico_seiga.niconicoSeiga(),
	owly.owly(),
	pckles.pckles(),
	photozou.photozou(),
	pixiv.pixiv(),
	tinami.tinami(),
	twipple.twipple(),
	twitgoo.twitgoo(),
	twitpic.twitpic(),
	twitvideo.twitvideo(),
	viame.viame(),
	yfrog.yfrog()
]
