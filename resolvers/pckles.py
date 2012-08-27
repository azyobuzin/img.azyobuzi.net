# -*- coding: utf-8 -*-

import re

class pckles:
	def __str__(self):
		return "Pckles"
	
	regexStr = "^https?://pckles\\.com/(\w+)/(\w+)(?:/|\\.png|\\.(?:resize\\.)?jpg)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://pckles.com/%s/%s.png" % (match.group(1), match.group(2))
	
	def getLargeSize(self, match):
		return "http://pckles.com/%s/%s.jpg" % (match.group(1), match.group(2))
	
	def getThumbnail(self, match):
		return "http://pckles.com/%s/%s.resize.jpg" % (match.group(1), match.group(2))
