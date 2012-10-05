# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import json
from private_constant import *

class ustream:
	def __str__(self):
		return "Ustream.tv"
	
	regexStr = "^(?:https?://(?:www\\.)?ustream\\.tv/channel/(?:([\\w\\-%]+)|id/(\\d+))|http://ustre\\.am/(\\w+)\\)?)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		channel = urllib2.unquote(match.group(1)).decode("utf-8") if match.group(1) is not None else None
		id = match.group(2)
		shorten = match.group(3)
		
		if shorten is not None:
			#http://azyobuzin.hatenablog.com/entry/2012/09/16/210350
			alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
			id = sum([len(alphabet) ** i * alphabet.index(shorten[len(shorten) - 1 - i]) for i in range(len(shorten))])
		
		if self.last is not None and (self.last[0] == channel or str(self.last[1]) == str(id)):
			return self.last
		
		id_str = str(channel if channel is not None else id)
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM ustream WHERE %s = %s"
			% ("channel" if channel is not None else "id", db.literal(id_str))
		)
		
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			#id_strは元々uri引数についてきたものだから、既にquoteされているものと判断
			httpRes = urllib2.urlopen("http://api.ustream.tv/json/channel/%s/getInfo?key=%s"
				% (urllib2.quote(id_str), ustreamApiKey))
			
			results = json.loads(httpRes.read())["results"]
			
			if results is None:
				return None
			
			id = results["id"]
			channel = results["urlTitleName"]
			
			imageUrl = results["imageUrl"]
			small = imageUrl["small"]
			medium = imageUrl["medium"]
			
			c.execute("INSERT INTO ustream VALUES (%s, %s, %s, %s)"
				% (db.literal(channel), db.literal(id), db.literal(small), db.literal(medium))
			)
			
			db.commit()
			
			self.last = [channel, id, small, medium]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[3] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[3] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
