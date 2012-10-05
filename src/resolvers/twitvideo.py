# -*- coding: utf-8 -*-

import re

class twitvideo:
	def __str__(self):
		return "twitvideo"
	
	regexStr = "^http://(?:www\\.)?twitvideo\\.jp/(?:img/\\w+/)?(\\w+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://twitvideo.jp/img/big/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://twitvideo.jp/img/display/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://twitvideo.jp/img/thumb/" + match.group(1)
