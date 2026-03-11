# Request to create an index in Elasticsearch

```json
PUT {{elasticURL}}/{{indexName}}
Authorization: ApiKey {{base64EncodedCredentials}}
Content-Type: application/json

{
  "settings": {
    "analysis": {
      "char_filter": {
        "remove_code_symbols": {
          "type": "pattern_replace",
          "pattern": "[{}();.'`\",:]",
          "replacement": " "
        }
      },
      "filter": {
        "code_stop": {
          "type": "stop",
          "stopwords": ["for", "while", "do", "if", "else", "return", "switch", "case", "break"]
        }
      },
      "analyzer": {
        "code_analyzer": {
          "type": "custom",
          "char_filter": ["remove_code_symbols"],
          "tokenizer": "whitespace",
          "filter": ["lowercase", "code_stop"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "name": { "type": "keyword" },
      "first_appeared": { "type": "date" },
      "is_statically_typed": { "type": "boolean" },
      "active_users": { "type": "integer" },
      "paradigms": { "type": "keyword" },

      "description": {
        "type": "text",
        "analyzer": "standard"
      },
      "history": {
        "type": "text",
        "analyzer": "english"
      },
      "code_example": {
        "type": "text",
        "analyzer": "code_analyzer"
      }
    }
  }
}
```

# Seeding the database

Before starting, fill the database with `seed.json` file.
Run the app and select `Seed the database` option in the main menu.

Make sure that `seed.json` exists in the same directore with app `.exe` file.

# Anylize the analyzer

```json
POST {{elasticURL}}/{{indexName}}/_analyze
Authorization: ApiKey {{base64EncodedCredentials}}
Content-Type: application/json

{
  "analyzer": "code_analyzer",
  "text": "const a = 10; const b = 20; console.log('c = ', a + b)"
}
```

# Send text search query

```json
GET {{elasticURL}}/{{indexName}}/_search
Authorization: ApiKey {{base64EncodedCredentials}}
Content-Type: application/json

{
  "query": {
    "multi_match": {
      "query": "concurrent programming language",
      "fields": ["description", "history"]
    }
  }
}
```
