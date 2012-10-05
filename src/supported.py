# -*- coding: utf-8 -*-

import sys
sys.path.append("resolvers")

import cloudapp
import dailymotion
import deviantart
import flickr
import fxcamera
import gochisophoto
import gyazo
import hatena_fotolife
import imgly
import imgur
import instagram
import lockerz
import mobypicture
import movapic
import mypix
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
import shamoji
import tinami
import tumblr
import tunabe
import twipple
import twitgoo
import twitpic
import twitrpix
import twitter
import twitvideo
import ustream
import viame
import vimeo
import yfrog
import youtube

#アルファベット順

services = [
	cloudapp.cloudapp(),
	dailymotion.dailymotion(),
	deviantart.deviantart(),
	flickr.flickr(),
	fxcamera.fxcamera(),
	gochisophoto.gochisophoto(),
	gyazo.gyazo(),
	hatena_fotolife.hatenaFotolife(),
	instagram.instagram(),
	imgly.imgly(),
	imgur.imgur(),
	lockerz.lockerz(),
	mobypicture.mobypicture(),
	movapic.movapic(),
	mypix.mypix(),
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
	shamoji.shamoji(),
	tinami.tinami(),
	tumblr.tumblr(),
	tunabe.tunabe(),
	twipple.twipple(),
	twitgoo.twitgoo(),
	twitpic.twitpic(),
	twitrpix.twitrpix(),
	twitter.twitter(),
	twitvideo.twitvideo(),
	ustream.ustream(),
	viame.viame(),
	vimeo.vimeo(),
	yfrog.yfrog(),
	youtube.youtube()
]
