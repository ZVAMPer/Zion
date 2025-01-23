# 1) Base image: Ubuntu, or Debian, or any minimal Linux with the libs you need
FROM ubuntu:20.04

# 2) Install dependencies your Unity headless build may require
#    (libc6, libgcc, libstdc++, libgomp, libssl, libcurl, etc.)
RUN apt-get update && \
    apt-get install -y \
    libstdc++6 \
    libgcc1 \
    libglu-dev \
    libssl1.1 \
    libcurl4 \
    # (Add other libs if your game complains they're missing)
    && apt-get clean

# 3) Create app directory
WORKDIR /app

# 4) Copy your server build into the container
#    Assume your build output is in a local folder named ServerBuilds
COPY ServerBuilds/ ./

# 5) Expose the port used by your Unity Netcode
#    For example, if your server uses port 7777 UDP, do:
EXPOSE 7777/udp

# 6) By default, just show usage. We'll override command at runtime.
CMD ["./MyGameServer.x86_64", "-mode", "server"]
