import os  
from openai import AzureOpenAI  
  
class OpenAIClient:  
    def __init__(self):  
        self.client = AzureOpenAI(  
            azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),  
            api_key=os.getenv("AZURE_OPENAI_KEY"),  
            api_version="2023-05-15"  
        )  
  
    def inference(self, content, prompt, max_tokens, model):  
        messages = [{"role": "system", "content": prompt}, {"role": "user", "content": content}]  
        raw_response = self.client.chat.completions.create(  
            model=model,   
            messages=messages,   
            temperature=0,   
            max_tokens=max_tokens  
        )  
        return raw_response.choices[0].message.content  
    
    def inferencestream(self, content, prompt, max_tokens, model):
        messages = [{"role": "system", "content": prompt}, {"role": "user", "content": content}]
        
        response = self.client.chat.completions.create(
            model=model,
            messages=messages,
            temperature=0,
            max_tokens=max_tokens,
            stream=True  # Enable streaming
        )

        for chunk in response:
            if chunk.choices[0].finish_reason  is not None:
                break
            if chunk.choices[0].delta.content != None:
                yield chunk.choices[0].delta.content
