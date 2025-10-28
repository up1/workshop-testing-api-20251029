from pydantic import ConfigDict
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    database_url: str
    secret_key: str
    environment: str = "development"
    log_level: str = "INFO"

    model_config = ConfigDict(env_file=".env")


settings = Settings()
