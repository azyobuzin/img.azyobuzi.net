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
import niconico
import niconico_seiga
import owly
import path
import pckles
import photoshare
import photozou
import piapro
import pikubo
import pixiv
import tinami
import twipple
import twitgoo
import twitpic
import twitvideo
import viame
import yfrog
import youtube

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
	niconico.niconico(),
	niconico_seiga.niconicoSeiga(),
	owly.owly(),
	path.path(),
	pckles.pckles(),
	photoshare.photoshare(),
	photozou.photozou(),
	piapro.piapro(),
	pikubo.pikubo(),
	pixiv.pixiv(),
	tinami.tinami(),
	twipple.twipple(),
	twitgoo.twitgoo(),
	twitpic.twitpic(),
	twitvideo.twitvideo(),
	viame.viame(),
	yfrog.yfrog(),
	youtube.youtube()
]
